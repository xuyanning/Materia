﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class SqrtNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public SqrtNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Square Root";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Input");
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);

            Outputs.Add(output);
        }

        public override void UpdateOutputType()
        {
            if(input.HasInput)
            {
                output.Type = input.Reference.Type;
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            NodeType t = input.Reference.Type;

            if (t == NodeType.Float)
            {
                return "float " + s + " = sqrt(" + n1id + ");\r\n";
            }
            else if(t == NodeType.Float2)
            {
                return "vec2 " + s + " = sqrt(" + n1id + ");\r\n";
            }
            else if(t == NodeType.Float3)
            {
                return "vec3 " + s + " = sqrt(" + n1id + ");\r\n";
            }
            else if(t == NodeType.Float4)
            {
                return "vec4 " + s + " = sqrt(" + n1id + ");\r\n";
            }

            return "";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;
            NodeType t = input.Reference.Type;

            try
            {
                if (t == NodeType.Float)
                {
                    float f = input.Data.ToFloat();
                    output.Data = (float)Math.Sqrt(f);
                }
                else if (t == NodeType.Float2 || t == NodeType.Float3 || t == NodeType.Float4)
                {
                    MVector v = (MVector)input.Data;
                    output.Data = v.Sqrt();
                }
                result = output.Data?.ToString();
            }
            catch (Exception e)
            {

            }

            UpdateOutputType();
        }
    }
}
