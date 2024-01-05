using System.Collections.Generic;
using System.Linq;

namespace IOCore.Libs
{
    public class DocumentFormat : FileFormat
    {
        public string Container { get; private set; }
        public string[] Extensions { get; private set; }

        public string Extension => Extensions.FirstOrDefault();

        public DocumentFormat(string name, string container, FileUtils.Type type, string[] extensions) : base(name, type)
        {
            Container = container;
            Extensions = extensions;
        }

        public DocumentFormat(DocumentFormat documentFormat) : base(documentFormat.Name, documentFormat.Type)
        {
            Container = documentFormat.Container;
            Extensions = documentFormat.Extensions.Clone() as string[];
        }
    }

    public class DocumentUtils
    {
        public enum FormatFamily
        {
            #region Text
            T_Doc,
            T_Pdf,
            T_Txt,
            T_Rtf,
            T_Odt,
            T_Pages,
            #endregion

            #region Presentation
            P_Ppt,
            P_Key,
            P_Odp,
            #endregion

            #region Sheet
            S_Xls,
            S_Ods,
            #endregion

            #region Ebook
            EB_Epub,
            EB_Mobi,
            EB_Azw,
            EB_Fb2,
            #endregion
        }

        public static readonly Dictionary<FormatFamily, DocumentFormat> DOCUMENT_FORMATS = new()
        {
            #region Text
            { FormatFamily.T_Doc,   new("DOC",      "doc",   FileUtils.Type.Document, new[] { ".doc", ".docx", ".docm" }  ) },
            { FormatFamily.T_Pdf,   new("PDF",      "pdf",   FileUtils.Type.Document, new[] { ".pdf" }                    ) },
            { FormatFamily.T_Txt,   new("TXT",      "txt",   FileUtils.Type.Document, new[] { ".txt", ".text", ".tex" }   ) },
            { FormatFamily.T_Rtf,   new("RTF",      "rtf",   FileUtils.Type.Document, new[] { ".rtf" }                    ) },
            { FormatFamily.T_Odt,   new("ODT",      "odt",   FileUtils.Type.Document, new[] { ".odt" }                    ) },
            { FormatFamily.T_Pages, new("pages",    "pages", FileUtils.Type.Document, new[] { ".pages" }                  ) },
            #endregion

            #region Presentation
            { FormatFamily.P_Ppt,   new("PPT",      "ppt",   FileUtils.Type.Document, new[] { ".ppt", ".pptx", ".pptm" }  ) },
            { FormatFamily.P_Key,   new("KEY",      "key",   FileUtils.Type.Document, new[] { ".key" }                    ) },
            { FormatFamily.P_Odp,   new("ODP",      "odp",   FileUtils.Type.Document, new[] { ".odp" }                    ) },
            #endregion

            #region Sheet
            { FormatFamily.S_Xls,   new("XLS",      "xls",   FileUtils.Type.Document, new[] { ".xls", ".xlsx", ".xlsm" }  ) },
            { FormatFamily.S_Ods,   new("ODS",      "ods",   FileUtils.Type.Document, new[] { ".ods" }                    ) },
            #endregion

            #region EBook
            { FormatFamily.EB_Epub, new("EPUB",     "epub",  FileUtils.Type.Document, new[] { ".epub" }                   ) },
            { FormatFamily.EB_Mobi, new("MOBI",     "mobi",  FileUtils.Type.Document, new[] { ".mobi" }                   ) },
            { FormatFamily.EB_Azw,  new("AZW",      "azw",   FileUtils.Type.Document, new[] { ".azw", ".azw3" }           ) },
            { FormatFamily.EB_Fb2,  new("FB2",      "fb2",   FileUtils.Type.Document, new[] { ".fb2" }                    ) },
            #endregion
        };

        public static readonly Dictionary<FormatFamily, DocumentFormat> TEXT_DOCUMENT_FORMATS;
        public static readonly Dictionary<string, string> CONTAINERS;

        static DocumentUtils()
        {
            TEXT_DOCUMENT_FORMATS = new();
            CONTAINERS = new();

            foreach (var m in DOCUMENT_FORMATS)
            {
                if (m.Value.Type == FileUtils.Type.Document)
                    TEXT_DOCUMENT_FORMATS.Add(m.Key, m.Value);

                foreach (var e in m.Value.Extensions)
                    if (!CONTAINERS.ContainsKey(e))
                        CONTAINERS.Add(e, m.Value.Container);
            }
        }
    }
}