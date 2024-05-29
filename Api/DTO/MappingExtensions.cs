﻿using Api.DTO.Comments;
using Api.DTO.Events;
using Api.DTO.Notifications;
using Api.DTO.Posts;
using Api.DTO.Reports;
using Api.DTO.Users;
using Domain.DataModel;

namespace Api.DTO;

public static class MappingExtensions
{
    public static PublicUserDto ToDto(this User user) =>
        new(user.Guid, user.FirstName, user.LastName, user.Description);

    public static PrivateUserDto ToPrivateDto(this User user) =>
        new(user.Guid, user.FirstName, user.LastName, user.Email, user.Description,
            user.DateOfBirth, user.IsAdmin, user.IsOrganizer, user.EmailNotification, user.ProfilePictureUrl);

    public static CommentDto ToDto(this Comment comment, User? acting) =>
        new(comment.Guid, comment.Author?.ToDto(),
            comment.Likes.SingleOrDefault(x => x.AuthorId == (acting?.Guid ?? Guid.Empty))?.IsDislike,
            comment.Likes.Count,
            comment.Content);

    public static PostDto ToDto(this Post post, User? acting) =>
        new(post.Guid, post.Content, post.EventId, post.Event.Title, post.Author?.ToDto(), post.CreationDate,
            post.Reactions.SingleOrDefault(x => x.AuthorId == (acting?.Guid ?? Guid.Empty))?.Type,
            post.Reactions.Count,
            post.Pictures.OrderBy(x => x.Index).Select(x => x.Url));

    public static ReactionDto ToDto(this Reaction reaction) =>
        new(reaction.Author.ToDto(), reaction.Type);

    public static EventDto ToDto(this Event @event)
    {
        int? avPlaces = null;
        if (@event.Capacity is not null)
            avPlaces = @event.Capacity - @event.Participants.Count;
        return new(@event.Guid, @event.Organizer?.ToDto(), @event.Title, @event.Description,
            @event.Category.ToString(), @event.StartDate, @event.EndDate,
            @event.Location, @event.Participants.Count, @event.Interested.Count, @event.ViewCount, @event.Fee, @event.Capacity, avPlaces, @event.AverageAge, null,
            @event.Pictures.OrderBy(x => x.Index).Select(x => x.Url));
    }

    public static ListEventDto ToListEventDto(this Event e)
    {
        int? avPlaces = null;
        if (e.Capacity is not null)
            avPlaces = e.Capacity - e.Participants.Count;
        return new ListEventDto(e.Guid, e.Title, e.StartDate, e.EndDate, e.Location, e.Participants.Count, e.Interested.Count, avPlaces, e.Fee, null, e.Pictures.OrderBy(x => x.Index).Select(x => x.Url));
    }

    public static NotificationDto ToDto(this BaseNotification notification) =>
        new(notification.Guid, notification.SourceId, notification.TypeString,
            notification.Seen, notification.Timestamp);

    public static FriendRequestDto ToDto(this FriendRequest friendRequest, bool asReceived) =>
        new(friendRequest.Guid, friendRequest.Timestamp,
            asReceived ? friendRequest.Author.ToDto() : friendRequest.Target.ToDto());
    public static ReportDto ToDto(this Report report) =>
    new(report.Guid, report.Author?.ToDto(), report.Responder?.ToDto(), report.TargetId, report.Title,
        report.Details, report.CreationDate, report.UpdateDate,
        report.Feedback, report.IsOpen, report.ReportType);
}