using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace TopEval
{
    public class TopEvalInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "TopEval";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                //return null;
                return Properties.Resources.TopEval_Icon;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Evaluate conformity of a topology with a given vector field.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("4318689b-edc8-4e87-ac02-0624b5316d3a");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Demi Fang";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "dfang@mit.edu";
            }
        }
    }
}
