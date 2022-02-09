using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace TopEval
{
    public class Conformity : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Conformity()
          : base("Evaluate Conformity", "Conformity",
              "Evaluate conformity of topology with a given vector field and neighborhood radius.",
              "TopEval", "Conformity")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Bars", "bars", "Bar elements to evaluate conformity against", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "pts",
                "Points at which stress field is provided. Must have one-to-one correspondence with s1 and s2 inputs",
                GH_ParamAccess.list);
            pManager.AddVectorParameter("Principal stress vector 1", "s1",
                "Primary principal stress vector. Must have one-to-one correspondence with pts and s2 inputs",
                GH_ParamAccess.list);
            pManager.AddVectorParameter("Principal stress vector 2", "s2",
                "Secondary principal stress vector. Must have one-to-one correspondence with pts and s1 inputs",
                GH_ParamAccess.list);
            pManager.AddBooleanParameter("Sign check", "sign",
                "Boolean check: are s1 and s2 the same sign?",
                GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "rad", "Radius of neighborhood to detect bar endpoints from each point",
                GH_ParamAccess.item, 0.1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddNumberParameter("Conformity", "conformity", "Conformity value for each point", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 1", "conform1", "Conformity value for each point, formulation 1", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 2", "conform2", "Conformity value for each point, formulation 2", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 3", "conform3", "Conformity value for each point, formulation 3", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 4", "conform4", "Conformity value for each point, formulation 4", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 5", "conform5", "Conformity value for each point, formulation 5", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 6", "conform6", "Conformity value for each point, formulation 6", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 7", "conform7", "Conformity value for each point, formulation 7", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 8", "conform8", "Conformity value for each point, formulation 8", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 9", "conform9", "Conformity value for each point, formulation 9", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 10", "conform10", "Conformity value for each point, formulation 10", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 11", "conform11", "Conformity value for each point, formulation 11", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 12", "conform12", "Conformity value for each point, formulation 12", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 13", "conform13", "Conformity value for each point, formulation 13", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 14", "conform14", "Conformity value for each point, formulation 14", GH_ParamAccess.list);
            pManager.AddNumberParameter("Formulation 15", "conform15", "Conformity value for each point, formulation 15", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Line> bars = new List<Line>();
            List<Point3d> pts = new List<Point3d>();
            List<Vector3d> sig1 = new List<Vector3d>();
            List<Vector3d> sig2 = new List<Vector3d>();
            List<bool> signCheck = new List<bool>();
            double tol = 0;

            if (!DA.GetDataList(0, bars)) return;
            if (!DA.GetDataList(1, pts)) return;
            if (!DA.GetDataList(2, sig1)) return;
            if (!DA.GetDataList(3, sig2)) return;
            if (!DA.GetDataList(4, signCheck)) return;
            if (!DA.GetData(5, ref tol)) return;

            List<List<double>> dist = new List<List<double>>();
            List<List<Vector3d>> bar_vecs = new List<List<Vector3d>>();
            List<bool> nearBar = new List<bool>();
            double k = 9.21; // e^-k is the value of conformity multiplier for a bar detected at d = radius = tol

            // for each node, create bar vectors from nearby bar endpoints
            // bar_vecs = data tree of bar vectors starting from endpoint near node i
            // dist = data tree of distances of each bar start point from node
            // nearBar = whether node i has bars or not
            for (int i = 0; i < pts.Count; i++)
            {
                Point3d p = pts[i];
                bool isEmpty = true;
                List<Vector3d> bar_vecs_sublist = new List<Vector3d>();
                List<double> dist_sublist = new List<double>();

                for (int j = 0; j < bars.Count; j++)
                {
                    Line bar = bars[j];
                    if (p.DistanceTo(bar.From) < tol)
                    {
                        Vector3d v = bar.To - bar.From;
                        bar_vecs_sublist.Add(v);
                        dist_sublist.Add(p.DistanceTo(bar.From));
                        isEmpty = false;
                    }
                    if (p.DistanceTo(bar.To) < tol)
                    {
                        Vector3d v = bar.From - bar.To;
                        bar_vecs_sublist.Add(v);
                        dist_sublist.Add(p.DistanceTo(bar.To));
                        isEmpty = false;
                    }
                }

                bar_vecs.Add(bar_vecs_sublist);
                dist.Add(dist_sublist);

                if (isEmpty)
                {
                    nearBar.Add(false);
                }
                else { nearBar.Add(true); }
            }

            // conformity calculation
            List<double> conformity1 = new List<double>();
            List<double> conformity2 = new List<double>();
            List<double> conformity3 = new List<double>();
            List<double> conformity4 = new List<double>();
            List<double> conformity5 = new List<double>();
            List<double> conformity6 = new List<double>();
            List<double> conformity7 = new List<double>();
            List<double> conformity8 = new List<double>();
            List<double> conformity9 = new List<double>();
            List<double> conformity10 = new List<double>();
            List<double> conformity11 = new List<double>();
            List<double> conformity12 = new List<double>();
            List<double> conformity13 = new List<double>();
            List<double> conformity14 = new List<double>();
            List<double> conformity15 = new List<double>();
            //int false_count = 0;

            for (int i = 0; i < sig1.Count; i++)
            {
                double conform1_node = 0;
                double conform2_node = 0;
                double conform3_node = 0;
                double conform4_node = 0;
                double conform5_node = 0;
                double conform6_node = 0;
                double conform7_node = 0;
                double conform8_node = 0;
                double conform9_node = 0;
                double conform10_node = 0;
                double conform11_node = 0;
                double conform12_node = 0;
                double conform13_node = 0;
                double conform14_node = 0;
                double conform15_node = 0;
                Vector3d s_A = sig1[i];
                Vector3d s_B = sig2[i];

                double r_A = s_A.Length;
                double r_B = s_B.Length;

                // only do this calculation if the branch has values; otherwise conform_node = 0
                if (!nearBar[i])
                {
                    //false_count = false_count + 1;
                    conformity1.Add(conform1_node);
                    conformity2.Add(conform2_node);
                    conformity3.Add(conform3_node);
                    conformity4.Add(conform4_node);
                    conformity5.Add(conform5_node);
                    conformity6.Add(conform6_node);
                    conformity7.Add(conform7_node);
                    conformity8.Add(conform8_node);
                    conformity9.Add(conform9_node);
                    conformity10.Add(conform10_node);
                    conformity11.Add(conform11_node);
                    conformity12.Add(conform12_node);
                    conformity13.Add(conform13_node);
                    conformity14.Add(conform14_node);
                    conformity15.Add(conform15_node);

                }
                else
                {
                    for (int j = 0; j < bar_vecs[i].Count; j++)
                    {
                        Vector3d bar_vec = bar_vecs[i][j];
                        double dist_j = dist[i][j];
                        double theta_j = 0;
                        double r_j = 0;
                        double piecewise_ind = 0; // 0 if more aligned with s1, 1 if more aligned with s2

                        // get angle between s1 and bar, as an angle between 0 and pi/2
                        // A and B indicate primary and secondary stress vectors
                        double theta_A = Math.Abs(Vector3d.VectorAngle(s_A, bar_vec));
                        if (theta_A > Math.PI / 2)
                        {
                            theta_A = Math.PI - theta_A;
                        }

                        // is the bar more aligned with s1 or s2?
                        if (theta_A <= Math.PI / 4)
                        {
                            theta_j = theta_A;
                            r_j = r_A;
                        }
                        else
                        {
                            theta_j = Math.PI / 2 - theta_A;
                            r_j = r_B;
                            piecewise_ind = 1;
                        }

                        double conform1_j = r_j * (-theta_j / (Math.PI / 4) + 1) * Math.Exp(-dist_j * k / tol); // formulation 1 and 3
                        //double conform2_j = Math.Pow(r_j, 2) * (-theta_j / (Math.PI / 4) + 1) * Math.Exp(-dist_j * k / tol); // formulation 2 and 4
                        double conform2_j = 0;
                        if (signCheck[i])
                        {
                            conform2_j = (-(r_A - r_B) / (Math.PI / 2) * theta_A + r_A) * Math.Exp(-dist_j * k / tol);
                        }
                        else 
                        {
                            conform2_j = Math.Abs(-(r_A + r_B) / (Math.PI / 2) * theta_A + r_A) * Math.Exp(-dist_j * k / tol);
                        }

                        double conform5_j = r_j * (-theta_j / (Math.PI / 4) + 1); // formulation 5


                        // formulation 7 and 9
                        //double conform7_j = (Math.Abs(Vector3d.Multiply(bar_vec, s_A)) + Math.Abs(Vector3d.Multiply(bar_vec, s_B))) / Math.Pow(dist_j, 2);
                        double conform7_j = (Math.Abs(Vector3d.Multiply(bar_vec, s_A)) + Math.Abs(Vector3d.Multiply(bar_vec, s_B))) * Math.Exp(-dist_j * k / tol);
                        double conform9_j = (Math.Abs(Vector3d.Multiply(bar_vec, s_A)) + Math.Abs(Vector3d.Multiply(bar_vec, s_B)));

                        // formulations 11, 12, 13
                        double conform11_j = 0;
                        double conform12_j = 0;
                        double conform13_j = 0;
                        if (piecewise_ind==0)
                        {
                            conform11_j = bar_vec.Length * s_A.Length * Math.Cos(4 * theta_A) * Math.Exp(-dist_j * k / tol);
                            conform12_j = bar_vec.Length * s_A.Length * Math.Cos(4 * theta_A) * Math.Exp(-dist_j * k / tol);
                            conform13_j = bar_vec.Length * s_A.Length * (-2 * Math.Sin(2 * theta_A) + 1) * Math.Exp(-dist_j * k / tol);
                        }
                        else
                        {
                            conform11_j = 0.5 * bar_vec.Length * s_A.Length * (Math.Cos(4 * theta_A) - 1) * Math.Exp(-dist_j * k / tol);
                            conform12_j = 0.5 * bar_vec.Length * ((s_A.Length + s_B.Length) * Math.Cos(4 * theta_A) - s_B.Length) * Math.Exp(-dist_j * k / tol);
                            conform13_j = bar_vec.Length * (-(s_A.Length + s_B.Length) * Math.Sin(2 * theta_A) + s_B.Length) * Math.Exp(-dist_j * k / tol);
                        }

                        // formulations 14 and 15
                        double conform14_j = s_A.Length * (-theta_A / (Math.PI / 2) + 1) * Math.Exp(-dist_j * k / tol); // formulation 14 and 15

                        // formulation 8 and 10 with unitized bar vecs. note the unitize operation!
                        bar_vec.Unitize();
                        //double conform8_j = (Math.Abs(Vector3d.Multiply(bar_vec, s_A)) + Math.Abs(Vector3d.Multiply(bar_vec, s_B))) / Math.Pow(dist_j, 2);
                        double conform8_j = (Math.Abs(Vector3d.Multiply(bar_vec, s_A)) + Math.Abs(Vector3d.Multiply(bar_vec, s_B))) * Math.Exp(-dist_j * k / tol);
                        double conform10_j = (Math.Abs(Vector3d.Multiply(bar_vec, s_A)) + Math.Abs(Vector3d.Multiply(bar_vec, s_B)));

                        // don't do any more formulation operations on bar_vec, it's been unitized!

                        conform1_node = conform1_node + conform1_j;
                        conform2_node = conform2_node + conform2_j;
                        conform5_node = conform5_node + conform5_j;
                        conform7_node = conform7_node + conform7_j;
                        conform8_node = conform8_node + conform8_j;
                        conform9_node = conform9_node + conform9_j;
                        conform10_node = conform10_node + conform10_j;
                        conform11_node = conform11_node + conform11_j;
                        conform12_node = conform12_node + conform12_j;
                        conform13_node = conform13_node + conform13_j;
                        conform14_node = conform14_node + conform14_j;

                    }
                    conform3_node = conform1_node / bar_vecs[i].Count; // average for formulation 3
                    conform4_node = conform2_node / bar_vecs[i].Count; // average for formulation 4
                    conform6_node = conform5_node / bar_vecs[i].Count; // average for formulation 6
                    conform7_node = conform7_node / bar_vecs[i].Count; // average for formulation 7
                    conform8_node = conform8_node / bar_vecs[i].Count; // average for formulation 8
                    conform9_node = conform9_node / bar_vecs[i].Count; // average for formulation 9
                    conform10_node = conform10_node / bar_vecs[i].Count; // average for formulation 10
                    conform11_node = conform11_node / bar_vecs[i].Count; // average for formulation 11
                    conform12_node = conform12_node / bar_vecs[i].Count; // average for formulation 12
                    conform13_node = conform13_node / bar_vecs[i].Count; // average for formulation 13
                    conform15_node = conform14_node / bar_vecs[i].Count; // average for formulation 15

                    conformity1.Add(conform1_node);
                    conformity2.Add(conform2_node);
                    conformity3.Add(conform3_node);
                    conformity4.Add(conform4_node);
                    conformity5.Add(conform5_node);
                    conformity6.Add(conform6_node);
                    conformity7.Add(conform7_node);
                    conformity8.Add(conform8_node);
                    conformity9.Add(conform9_node);
                    conformity10.Add(conform10_node);
                    conformity11.Add(conform11_node);
                    conformity12.Add(conform12_node);
                    conformity13.Add(conform13_node);
                    conformity14.Add(conform14_node);
                    conformity15.Add(conform15_node);
                }
            }


            DA.SetDataList(0, conformity1);
            DA.SetDataList(1, conformity2);
            DA.SetDataList(2, conformity3);
            DA.SetDataList(3, conformity4);
            DA.SetDataList(4, conformity5);
            DA.SetDataList(5, conformity6);
            DA.SetDataList(6, conformity7);
            DA.SetDataList(7, conformity8);
            DA.SetDataList(8, conformity9);
            DA.SetDataList(9, conformity10);
            DA.SetDataList(10, conformity11);
            DA.SetDataList(11, conformity12);
            DA.SetDataList(12, conformity13);
            DA.SetDataList(13, conformity14);
            DA.SetDataList(14, conformity15);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                //return null;
                return Properties.Resources.TopEval_Icon;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("560dafae-8573-4e5e-a61d-59349a7f392b"); }
        }
    }
}
