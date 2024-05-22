﻿using Domain.BaseTypes;
using Domain.DataModel;

namespace Domain.Services.Abstractions;

public interface IPictureService : IBaseService<IPictureService>
{
    public void UploadUserProfilePicture(Stream file);
    public void DeleteUserProfilePicture();

    public void UploadEventPicture(Guid eventGuid, int index, Stream file);
    public void DeleteEventPicture(Guid eventGuid, int index);

    public void UploadPostPicture(Guid postGuid, int index, Stream file);
    public void DeletePostPicture(Guid postGuid, int index);
}
