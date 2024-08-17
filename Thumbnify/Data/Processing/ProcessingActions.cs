using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Material.Icons;
using Thumbnify.Data.Processing.Audio;
using Thumbnify.Data.Processing.Converters;
using Thumbnify.Data.Processing.Input;
using Thumbnify.Data.Processing.Operations;
using Thumbnify.Data.Processing.Video;
using Thumbnify.Data.Processing.Youtube;

namespace Thumbnify.Data.Processing {
    internal static class ProcessingActions {
        public static List<ProcessingGroup> Actions { get; } = new() {
            new("mnu_input", MaterialIconKind.SourceCommitStart, [
                new(MediaPartInput.Id, MaterialIconKind.Multimedia, typeof(MediaPartInput)),
                new(PathInput.Id, MaterialIconKind.Folder, typeof(PathInput)),
                new(TempPathInput.Id, MaterialIconKind.FolderPound, typeof(TempPathInput)),
                new(StringInput.Id, MaterialIconKind.FormTextbox, typeof(StringInput)),
                new(ThumbnailInput.Id, MaterialIconKind.Image, typeof(ThumbnailInput))
            ]),
            new("mnu_video", MaterialIconKind.MovieOpen, [
                new(VideoSaveFile.Id, MaterialIconKind.ContentSave, typeof(VideoSaveFile)),
                new(YoutubeUpload.Id, MaterialIconKind.CloudUpload, typeof(YoutubeUpload))
            ]),
            new("mnu_audio", MaterialIconKind.Music, [
                new(AudioLoadFile.Id, MaterialIconKind.FolderOpen, typeof(AudioLoadFile)),
                new(AudioSaveFile.Id, MaterialIconKind.ContentSave, typeof(AudioSaveFile)),
                null,
                new(AudioMerge.Id, MaterialIconKind.CallMerge, typeof(AudioMerge)),
                new(AudioResample.Id, MaterialIconKind.SineWave, typeof(AudioResample)),
                new(AudioNormalizer.Id, MaterialIconKind.Waveform, typeof(AudioNormalizer)),
                new(AudioCompressor.Id, MaterialIconKind.ArrowCollapseVertical,
                    typeof(AudioCompressor)),
                null,
                new(AudioMetadata.Id, MaterialIconKind.Tag, typeof(AudioMetadata))
            ]),
            new("mnu_youtube", MaterialIconKind.VideoBox, [
                new(YoutubeUpload.Id, MaterialIconKind.CloudUpload, typeof(YoutubeUpload)),
                new(YoutubeAddPlaylist.Id, MaterialIconKind.ViewList, typeof(YoutubeAddPlaylist))
            ]),
            new("mnu_files", MaterialIconKind.Folder, []),
            new("mnu_convert", MaterialIconKind.SwapHorizontal, [
                new(ConvertDate.Id, MaterialIconKind.Calendar, typeof(ConvertDate))
            ]),
            new("mnu_operations", MaterialIconKind.Cog, [
                new(PathCombine.Id, MaterialIconKind.FileTree, typeof(PathCombine)),
                new(RenderThumbnail.Id, MaterialIconKind.Image, typeof(RenderThumbnail))
            ])
        };
    }

    public record ProcessingAction(string TranslationKey, MaterialIconKind Icon, Type Type);

    public record ProcessingGroup(string TranslationKey, MaterialIconKind Icon, List<ProcessingAction?> Items);
}