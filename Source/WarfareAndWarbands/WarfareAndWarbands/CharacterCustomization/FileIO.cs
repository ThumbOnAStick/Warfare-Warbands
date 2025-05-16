using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace WarfareAndWarbands.CharacterCustomization
{
    [StaticConstructorOnStartup]
    public static class FileIO
    {
        private static string currentExtraDefPath;

        static FileIO()
        {
            var appendix = "\\Defs\\Extra.xml";
            currentExtraDefPath = Directory.GetParent(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString()) + appendix;

            Log.Message($"WAW: Current directory: {currentExtraDefPath}");
        
        }

        public static void WriteDefs(string defName)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {

                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false
            };

            using (XmlReader reader = XmlReader.Create(currentExtraDefPath))
            {
                while (reader.Read())
                {
                    if (reader.GetAttribute("defName") == defName)
                    {
                        Log.Message($"WAW: {defName} Already written");
                        reader.Close();
                        return;
                    }
                }
            }

            using (XmlWriter writer = XmlWriter.Create(currentExtraDefPath, settings))
            {

                // Write XML declaration
                writer.WriteStartDocument();

                // Write root element: <Defs>
                writer.WriteStartElement("Defs");

                foreach(var ele in GameComponent_Customization.Instance.GeneratedKindDefs)
                {
                    // Write <PawnKindDef> element
                    writer.WriteStartElement("PawnKindDef");
                    writer.WriteAttributeString("ParentName", "MercenaryBase");
                    writer.WriteElementString("defName", ele.defName);
                    writer.WriteElementString("combatPower", "100");
                    writer.WriteElementString("initialResistanceRange", "10~16");
                    writer.WriteElementString("initialWillRange", "2~4");

                    // Close <PawnKindDef>
                    writer.WriteEndElement();
                }

                // Close <Defs>
                writer.WriteEndElement();

                // End document
                writer.WriteEndDocument();
            }

            Log.Message($"WAW: XML file written to {currentExtraDefPath}");
        }
    }
}
