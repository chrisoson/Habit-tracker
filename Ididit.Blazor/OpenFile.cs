﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Ididit.Blazor;

public class OpenFile : IOpenFile
{
    public RenderFragment CreateFilePicker(Action<Stream> onFilePicked)
    {
        return builder =>
        {
            builder.OpenComponent(0, typeof(InputFile));
            builder.AddAttribute(1, "OnChange", EventCallback.Factory.Create(this, (InputFileChangeEventArgs args) =>
            {
                Stream stream = args.File.OpenReadStream();
                onFilePicked(stream);
            }));
            builder.CloseComponent();
        };
    }
}
