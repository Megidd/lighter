﻿using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using System.IO;
using System;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Text.Json;

namespace Feather
{
    public class FeatherLighten : Command
    {
        public FeatherLighten()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static FeatherLighten Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "FeatherLighten";

        private static RhinoDoc docCurrent; // Accessed by async post-process code.
        private static RhinoObject inObj = null; // Input object.
        private static string inPath = Path.GetTempPath() + "input.stl"; // Input object to be saved as STL.

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            docCurrent = doc; // Accessed by async post-process code.
            inObj = Helper.GetInputStl(inPath);
            if (inObj == null)
            {
                return Result.Failure;
            }

            List<Point3d> loadPoints = Helper.GetPointOnMesh(inObj, "Select load/force points on mesh (Esc to cancel)");
            if (loadPoints == null || loadPoints.Count < 1)
            {
                RhinoApp.WriteLine("No points are selected");
                return Result.Failure;
            }

            RhinoApp.WriteLine("Load/force points count: {0}", loadPoints.Count);

            List<Point3d> restraintPoints = Helper.GetPointOnMesh(inObj, "Select restraint points on mesh (Esc to cancel)");
            if (restraintPoints == null || restraintPoints.Count < 1)
            {
                RhinoApp.WriteLine("No points are selected");
                return Result.Failure;
            }

            RhinoApp.WriteLine("Restraint points count: {0}", restraintPoints.Count);

            string loadJson = JsonSerializer.Serialize(loadPoints);
            File.WriteAllText(Path.GetTempPath() + "loadPoints.json", loadJson);

            string restraintJson = JsonSerializer.Serialize(restraintPoints);
            File.WriteAllText(Path.GetTempPath() + "restraintPoints.json", restraintJson);

            // Prepare arguments as text fields.
            string args = "";
            args += "lighten";
            args += " ";
            args += inPath;

            Helper.RunLogic(args, PostProcess);

            RhinoApp.WriteLine("Process started. Please wait...");

            return Result.Success;
        }

        private static void PostProcess(object sender, EventArgs e)
        {
            try
            {
                RhinoApp.WriteLine("Post process started.");
                RhinoApp.WriteLine("Post process finished.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("Error on post process: {0}", ex.Message);
            }
        }
    }
}
