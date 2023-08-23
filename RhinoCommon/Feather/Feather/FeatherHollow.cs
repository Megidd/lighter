﻿using System;
using System.IO;
using Rhino;
using Rhino.Commands;

namespace Feather
{
    public class FeatherHollow : Command
    {
        public FeatherHollow()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static FeatherHollow Instance { get; private set; }

        public override string EnglishName => "FeatherHollow";

        private static string outPath = System.IO.Path.GetTempPath() + "output.stl"; // Abs path is easier.

        private static RhinoDoc docCurrent; // Accessed by async post-process code.

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // To be accessed by async post-process code.
            docCurrent = doc;

            string inPath = System.IO.Path.GetTempPath() + "input.stl"; // Abs path is easier.

            if (Helper.GetInputStl(inPath) == Result.Failure)
            {
                return Result.Failure;
            }

            bool infill = Helper.GetYesNoFromUser("Do you want infill for hollowed mesh?");

            float thickness = Helper.GetFloatFromUser(1.8, 0.0, 100.0, "Enter wall thickness for hollowing.");

            uint precision = Helper.GetUint32FromUser("Enter precision: Low=1, Medium=2, High=3", 2, 1, 3);
            switch (precision)
            {
                case 1:
                case 2:
                case 3:
                    break;
                default:
                    RhinoApp.WriteLine("Precision must be 1, 2, or 3 i.e. Low=1, Medium=2, High=3");
                    return Result.Failure;
            }

            // Prepare arguments as text fields.
            string args = inPath;
            args += " ";
            args += infill ? "true" : "false";
            args += " ";
            args += thickness.ToString();
            args += " ";
            args += precision.ToString();
            args += " ";
            args += outPath;

            Helper.RunLogic(args, PostProcess);

            RhinoApp.WriteLine("Process is started. Please wait...");

            return Result.Success;
        }

        private static void PostProcess(object sender, EventArgs e)
        {
            try
            {
                RhinoApp.WriteLine("Post process is started for {0}", outPath);
                String ext = Path.GetExtension(outPath);
                if (null == ext || !ext.ToLower().Equals(".stl", StringComparison.OrdinalIgnoreCase))
                {
                    RhinoApp.WriteLine("Post process: file type must be STL:", outPath);
                    return;
                }

                // Import output STL and add it to the current 3D scene.
                bool good = docCurrent.Import(outPath);
                if (!good)
                {
                    RhinoApp.WriteLine("Post process: output file cannot be imported: {0}", outPath);
                }
            }

            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error on post process: {0}", ex.Message);
            }
        }
    }
}