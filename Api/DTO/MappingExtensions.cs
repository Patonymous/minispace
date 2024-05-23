﻿using Api.DTO.Comments;
using Api.DTO.Events;
using Api.DTO.Notifications;
using Api.DTO.Posts;
using Api.DTO.Reports;
using Api.DTO.Students;
using Api.DTO.Users;
using Domain.DataModel;
using Domain.Services;

namespace Api.DTO;

public static class MappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(user.Guid, user.FirstName, user.LastName, user.Email, user.Description,
            user.DateOfBirth, user.IsAdmin, user.IsOrganizer, user.EmailNotification);

    public static CommentDto ToDto(this Comment comment) =>
        new(comment.Guid, comment.Author?.ToDto(), comment.Content);

    public static PostDto ToDto(this Post post) =>
        new(post.Guid, post.EventId, post.Author?.ToDto(), post.CreationDate);

    public static EventDto ToDto(this Event @event) =>
        new(@event.Guid, @event.Organizer?.ToDto(), @event.Title, @event.Description,
            @event.Category.ToString(), @event.PublicationDate, @event.StartDate, @event.EndDate,
            @event.Location, @event.Participants.Count, @event.Interested.Count, @event.ViewCount, @event.AverageAge);

    public static NotificationDto ToDto(this BaseNotification notification) =>
        new(notification.Guid, notification.SourceId, notification.TypeString,
            notification.Seen, notification.Timestamp);

    public static FriendRequestDto ToDto(this FriendRequest friendRequest) =>
        new(friendRequest.Guid, friendRequest.TargetId, friendRequest.AuthorId, friendRequest.Timestamp);

    public static ReportDto ToDto(this Report report) =>
        new(report.Guid, report.Author?.ToDto(), report.Responder?.ToDto(), report.TargetId, report.Title,
            report.Details, report.Category.ToString(), report.CreationDate, report.UpdateDate,
            report.Feedback, report.State.ToString(), report.ReportType.ToString());

    public static PagedResponse<R> Map<T, R>(this PagedResponse<T> paged, Func<T, R> mappingFunction) =>
        new()
        {
            Items = paged.Items.Select(mappingFunction),
            PageIndex = paged.PageIndex,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            IsLast = paged.IsLast
        };
}