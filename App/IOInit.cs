#if INIT
using System.Collections.Generic;
using System.IO;
using System;
using IOApp.Configs;
using IOCore.Libs;
using IOCore;

namespace IOApp
{
    internal class IOInit
    {
        private class ContextMenuVerb
        {
            public string Id;
            public string Switch;
            public string Content;

            public ContextMenuVerb(string vId, string vSwitch, string vContent)
            {
                Id = vId;
                Switch = vSwitch;
                Content = vContent;
            }
        }

        private class ContextMenuItem
        {
            public string Extension;
            public ContextMenuVerb[] Verbs;

            public ContextMenuItem(string extension, ContextMenuVerb[] verbs)
            {
                Extension = extension;
                Verbs = verbs;
            }
        }

        private static void RegisterExtensions()
        {
            var contextMenuItems = new List<ContextMenuItem>();

            //foreach (var ext in Profile.INPUT_MEDIA_EXTENSIONS)
            //{
            //    var verbs = new List<ContextMenuVerb>()
            //    {
            //        new(App.CommandType.Play.ToString(), "/play", $"Play with {Windows.ApplicationModel.Package.Current?.DisplayName}"),
            //        new(App.CommandType.ConvertToMp3.ToString(), "/convertToMp3", $"Convert to MP3"),
            //    };

            //    if (FileUtils.GetTypeByExtension(ext) == FileUtils.Type.Video)
            //        verbs.Add(new(App.CommandType.ConvertToMp4.ToString(), "/convertToMp4", $"Convert to MP4"));

            //    contextMenuItems.Add(new(ext, verbs.ToArray()));
            //}

            var extensionTemplate = "<uap3:Extension Category=\"windows.fileTypeAssociation\"><uap3:FileTypeAssociation Name=\"%extension%\" Parameters=\"&quot;%1&quot;\"><uap:SupportedFileTypes><uap:FileType>.%extension%</uap:FileType></uap:SupportedFileTypes><uap2:SupportedVerbs>%verbs%</uap2:SupportedVerbs></uap3:FileTypeAssociation></uap3:Extension>";
            var verbTemplate = "<uap3:Verb Id=\"%verbId%\" Parameters=\"&quot;%1&quot; %verbSwitch%\">%verbContent%</uap3:Verb>";
            var path = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.Parent.FullName, "Package.appxmanifest");
            var document = XmlUtils.LoadXmlDocument(path);

            var node = document.GetElementsByTagName("Extensions")[0];
            node.RemoveAll();

            var extensions = new List<string>
            {
                $"<uap5:Extension Category=\"windows.startupTask\"><uap5:StartupTask TaskId=\"{Meta.IO_APP_ID}\" Enabled=\"false\" DisplayName=\"{(global::Windows.ApplicationModel.Package.Current?.DisplayName)}\"/></uap5:Extension>"
            };

            foreach (var item in contextMenuItems)
            {
                var verbs = new List<string>();

                foreach (var verb in item.Verbs)
                    verbs.Add(verbTemplate.Replace("%verbId%", verb.Id).Replace("%verbSwitch%", verb.Switch).Replace("%verbContent%", verb.Content));

                extensions.Add(extensionTemplate.Replace("%extension%", item.Extension.Remove(0, 1)).Replace("%verbs%", string.Join("\n", verbs)));
            }

            node.InnerXml = string.Join("\n", extensions);

            document.Save(path);
        }

        private static void PrecompileModels()
        {
            var parentPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.Parent.Parent.FullName;
            var batFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bat");

            using (var batFile = new StreamWriter(batFilePath))
            {
                var version = "7.0.14";
                batFile.WriteLine($"chdir /d {parentPath}");
                batFile.WriteLine($"dotnet tool install --global dotnet-ef --version {version}");
                batFile.WriteLine($"dotnet tool update --global dotnet-ef --version {version}");
                batFile.WriteLine($"dotnet ef dbcontext optimize --configuration Init --project App --namespace IOApp.CompiledModels --output-dir CompiledModels");
            }

            var p = new System.Diagnostics.Process()
            {
                StartInfo = new()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{batFilePath}\"",
                },
            };

            p.Start();
            p.WaitForExit();

            Utils.DeleteFileOrDirectory(batFilePath);
        }

        public static void Init()
        {
            PrecompileModels();
            //RegisterExtensions();
        }
    }
}
#endif