﻿namespace Api.DTO.Posts;

public record CreatePost(
    Guid EventGuid,
    string Title,
    string Content);