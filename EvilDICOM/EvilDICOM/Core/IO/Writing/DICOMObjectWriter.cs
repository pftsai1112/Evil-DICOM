#region

using EvilDICOM.Core.Helpers;
using EvilDICOM.Core.Interfaces;
using EvilDICOM.Core.Logging;
using Microsoft.Extensions.Logging;
using System;

#endregion

namespace EvilDICOM.Core.IO.Writing
{
    public class DICOMObjectWriter
    {
        static ILogger _logger = EvilLogger.LoggerFactory.CreateLogger<DICOMObjectWriter>();
        public static bool IsFileMetaGroup(IDICOMElement el)
        {
            return el.Tag.Group == "0002";
        }

        public static void Write(DICOMBinaryWriter dw, DICOMIOSettings settings, DICOMObject d,
            bool isSequenceItem = false)
        {
            if (!isSequenceItem) TransferSyntaxHelper.SetSyntax(d, settings.TransferSyntax);

            for (var i = 0; i < d.Elements.Count; i++)
            {
                var el = d.Elements[i];
                var currentSettings = IsFileMetaGroup(el) ? settings.GetFileMetaSettings() : settings;
                if (GroupWriter.IsGroupHeader(el))
                {
                    var skip = GroupWriter.WriteGroup(dw, currentSettings, d, el);
                    i += skip;
                }
                else
                {
                    _logger.LogInformation($"Writing element ${el.Tag.CompleteID}");
                    try
                    {
                        DICOMElementWriter.Write(dw, currentSettings, el);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error writing :  ${el.Tag.CompleteID}\n{e}");
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// Ignores the rule of writing metadata in explicit VR little endian and instead writes all elements with the same passed in syntax
        /// Used in PData writing
        /// </summary>
        /// <param name="dw"></param>
        /// <param name="settings"></param>
        /// <param name="d"></param>
        /// <param name="isSequenceItem"></param>
        public static void WriteSameSyntax(DICOMBinaryWriter dw, DICOMIOSettings settings, DICOMObject d,
            bool isSequenceItem = false)
        {
            for (var i = 0; i < d.Elements.Count; i++)
            {
                var el = d.Elements[i];
                if (GroupWriter.IsGroupHeader(el))
                {
                    var skip = GroupWriter.WriteGroup(dw, settings, d, el);
                    i += skip;
                }
                else
                {
                    DICOMElementWriter.Write(dw, settings, el);
                }
            }
        }
    }
}