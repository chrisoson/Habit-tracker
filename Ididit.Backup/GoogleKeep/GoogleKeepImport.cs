﻿using Ididit.Data;
using Ididit.Data.Models;
using System.IO.Compression;
using System.Text.Json;

namespace Ididit.Backup.GoogleKeep;

public class GoogleKeepImport(AppData appData)
{
    private readonly AppData _appData = appData;

    public async Task ImportDataFile(Stream stream)
    {
        List<GoogleKeepNote> googleKeepNotes = await GetGoogleKeepNotes(stream);

        UserData userData = new();

        CategoryModel category = new();
        NoteModel? note = null;
        TaskModel? task = null;
        HabitModel? habit = null;

        DateTime now = DateTime.Now;

        foreach (GoogleKeepNote googleKeepNote in googleKeepNotes.OrderByDescending(gkn => gkn.CreatedTimestampUsec))
        {
            if (googleKeepNote.Labels.Count > 0)
            {
                Label label = googleKeepNote.Labels.First();

                if (userData.Categories.FirstOrDefault(x => x.Title == label.Name) is CategoryModel categoryModel)
                {
                    category = categoryModel;
                }
                else
                {
                    category = new() { Title = label.Name };

                    userData.Categories.Add(category);
                }
            }

            category.Notes ??= new();
            category.Tasks ??= new();
            category.Habits ??= new();

            if (googleKeepNote.ListContent.Count == 0)
            {
                note = new()
                {
                    Title = googleKeepNote.Title,
                    Content = googleKeepNote.TextContent,
                    IsDeleted = googleKeepNote.IsTrashed,
                    //Color = googleKeepNote.Color
                    CreatedAt = new DateTime(googleKeepNote.CreatedTimestampUsec),
                    UpdatedAt = new DateTime(googleKeepNote.UserEditedTimestampUsec)
                };

                category.Notes.Add(note);
            }
            else
            {
                task = new()
                {
                    Title = googleKeepNote.Title,
                    IsDeleted = googleKeepNote.IsTrashed,
                    //Color = googleKeepNote.Color
                    CreatedAt = new DateTime(googleKeepNote.CreatedTimestampUsec),
                    UpdatedAt = new DateTime(googleKeepNote.UserEditedTimestampUsec)
                };

                if (googleKeepNote.ListContent.Count > 0)
                {
                    task.Items = new();

                    foreach (ListContent listContent in googleKeepNote.ListContent)
                    {
                        ItemModel item = new()
                        {
                            Title = listContent.Text,
                            DoneAt = listContent.IsChecked ? now : null
                        };

                        task.Items.Add(item);
                    }
                }

                category.Tasks.Add(task);
            }
        }

        await _appData.SetUserData(userData);
    }

    private static async Task<List<GoogleKeepNote>> GetGoogleKeepNotes(Stream stream)
    {
        MemoryStream memoryStream = new();

        await stream.CopyToAsync(memoryStream);

        ZipArchive archive = new(memoryStream);

        List<GoogleKeepNote> googleKeepNotes = new();

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                await using Stream jsonStream = entry.Open();

                using StreamReader streamReader = new(jsonStream);

                string jsonText = await streamReader.ReadToEndAsync();

                GoogleKeepNote? googleKeepNote = JsonSerializer.Deserialize<GoogleKeepNote>(jsonText);

                if (googleKeepNote is not null)
                {
                    googleKeepNotes.Add(googleKeepNote);
                }
            }
        }

        return googleKeepNotes;
    }
}
