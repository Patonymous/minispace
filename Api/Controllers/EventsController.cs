﻿using Api.DTO;
using Api.DTO.Events;
using Api.DTO.Posts;
using Domain.DataModel;
using Domain.Services;
using Domain.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[Route("events")]
[ApiController]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation("List all events")]
    public ActionResult<Paged<ListEventDto>> GetEvents([FromQuery] Paging paging, [FromQuery] GetEventsFilters f)
    {
        var events = eventService.AsUser(User.TryGetGuid()).GetAll();
        events = Filter(events,
            f.Time?.OfType<TimeType>(), f.Participants?.OfType<ParticipantsType>(),
            f.Price?.OfType<PriceType>(), f.EventName, f.OrganizerName, f.OnlyAvailablePlace);

        var paged = Paged<ListEventDto>.PageFrom(events.Select(e => e.ToListEventDto()),
            EventStateComparer.Instance, paging);

        return Ok(paged);
    }

    [HttpPost]
    [Authorize]
    [SwaggerOperation("Create event")]
    public ActionResult<EventDto> CreateEvent(CreateEvent newEvent)
    {
        var @event = eventService.AsUser(User.GetGuid()).CreateEvent(newEvent.Title, newEvent.Description, newEvent.EventCategory,
            newEvent.PublicationDate, newEvent.StartDate, newEvent.EndDate, newEvent.Location, newEvent.Capacity, newEvent.Fee);
        return Ok(@event.ToDto(eventService.ActingUser));
    }

    [HttpGet]
    [Route("{id}")]
    [SwaggerOperation("Details of given event")]
    public ActionResult<EventDto> GetEvent(Guid id)
    {
        var @event = eventService.AsUser(User.TryGetGuid()).GetEvent(id);
        return Ok(@event.ToDto(eventService.ActingUser));
    }

    [HttpGet]
    [Route("{id}/posts")]
    [SwaggerOperation("List event's posts")]
    public ActionResult<Paged<PostDto>> GetEventPosts([FromQuery] Paging paging, [FromRoute] Guid id)
    {
        var @event = eventService.AsUser(User.GetGuid()).GetEvent(id);
        return Paged<PostDto>.PageFrom(@event.Posts.Select(p => p.ToDto()), CreationDateComparer.Instance, paging);
    }

    [HttpDelete]
    [Authorize]
    [Route("{id}")]
    [SwaggerOperation("Delete event")]
    public ActionResult DeleteEvent(Guid id)
    {
        eventService.AsUser(User.GetGuid()).DeleteEvent(id);
        return Ok();
    }

    [HttpPost]
    [Authorize]
    [Route("{id}/participants")]
    [SwaggerOperation("Register for event")]
    public ActionResult<bool> RegisterForEvent(Guid id)
    {
        return Ok(eventService.AsUser(User.GetGuid()).TryAddParticipant(id));
    }

    [HttpDelete]
    [Authorize]
    [Route("{id}/participants")]
    [SwaggerOperation("Unregister from event")]
    public ActionResult<bool> UnregisterFromEvent(Guid id)
    {
        return Ok(eventService.AsUser(User.GetGuid()).TryRemoveParticipant(id));
    }

    [HttpPost]
    [Authorize]
    [Route("{id}/interested")]
    [SwaggerOperation("Show interest in event")]
    public ActionResult<bool> ShowInterestInEvent(Guid id)
    {
        return Ok(eventService.AsUser(User.GetGuid()).TryAddInterested(id));
    }

    [HttpDelete]
    [Authorize]
    [Route("{id}/interested")]
    [SwaggerOperation("Remove interest from event")]
    public ActionResult<bool> RemoveInterestInEvent(Guid id)
    {
        return Ok(eventService.AsUser(User.GetGuid()).TryRemoveInterested(id));
    }

    [HttpPost]
    [Authorize]
    [Route("{id}/feedback")]
    public ActionResult<Feedback> AddFeedback(Guid id, int rating)
    {
        return Ok(eventService.AsUser(User.GetGuid()).AddFeedback(id, rating));
    }

    private static IEnumerable<Event> Filter(IEnumerable<Event> events, IEnumerable<TimeType>? time,
         IEnumerable<ParticipantsType>? participants, IEnumerable<PriceType>? price,
         string? evNameFilter, string? orgNameFilter, bool onlyAvailablePlace)
    {
        // Event name filter
        if (!string.IsNullOrEmpty(evNameFilter))
            events = events.Where(e => e.Title.Contains(evNameFilter));

        // Organizer name filter
        if (!string.IsNullOrEmpty(orgNameFilter))
        {
            var name = orgNameFilter.Split();
            string firstName = name[0];
            string lastName = string.Empty;
            if (name.Length > 1)
                lastName = name[1];
            events = events.Where(e => e.Organizer is not null && e.Organizer.FirstName.Contains(firstName) && e.Organizer.LastName.Contains(lastName));
        }

        // Number of participants filter
        if (participants is not null && participants.Any() && participants.Count() < 3)
        {
            if (participants.Count() == 1)
            {
                events = participants.First() switch
                {
                    ParticipantsType.To50 => events.Where(x => x.Participants.Count <= 50 && x.Participants.Count >= 0),
                    ParticipantsType.From50To100 => events.Where(x => x.Participants.Count >= 50 && x.Participants.Count <= 100),
                    ParticipantsType.Above100 => events.Where(x => x.Participants.Count >= 100),
                    _ => throw new InvalidOperationException()
                };
            }
            else
            {
                if (!participants.Contains(ParticipantsType.To50))
                    events = events.Where(x => x.Participants.Count >= 50);
                else if (!participants.Contains(ParticipantsType.Above100))
                    events = events.Where(x => x.Participants.Count >= 0 && x.Participants.Count <= 100);
                else if (!participants.Contains(ParticipantsType.From50To100))
                    events = events.Where(x => (x.Participants.Count >= 0 && x.Participants.Count <= 50) || (x.Participants.Count >= 100));
            }
        }

        // Time filter
        if (time is not null && time.Any() && time.Count() < 3)
        {
            if (time.Count() == 1)
            {
                events = time.First() switch
                {
                    TimeType.Past => events.Where(x => x.EndDate <= DateTime.Now),
                    TimeType.Current => events.Where(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now),
                    TimeType.Future => events.Where(x => x.StartDate >= DateTime.Now),
                    _ => throw new InvalidOperationException()
                };
            }
            else
            {
                if (!time.Contains(TimeType.Past))
                    events = events.Where(x => x.EndDate >= DateTime.Now);
                else if (!time.Contains(TimeType.Future))
                    events = events.Where(x => x.StartDate <= DateTime.Now);
                else if (!time.Contains(TimeType.Current))
                    events = events.Where(x => (x.EndDate <= DateTime.Now) || (x.StartDate >= DateTime.Now));
            }
        }

        // Price filter
        if (price is not null && price.Any() && price.Count() < 2)
            events = events.Where(x => price.First() == PriceType.Free ? x.Fee is null || x.Fee == 0 : x.Fee is not null || x.Fee > 0);

        // Only events with available placces
        if (onlyAvailablePlace)
            events = events.Where(e => e.Capacity is null || (e.Capacity - e.Participants.Count > 0));

        return events;
    }
}
