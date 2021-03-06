﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes;
using Materia.Imaging;
using Materia.Imaging.GLProcessing;
using System.Threading;
using Materia.Nodes.Attributes;
using Materia.GLInterfaces;
using Materia.Textures;
using Newtonsoft.Json;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.Atomic
{
    public class HSLNode : ImageNode 
    {
        NodeInput input;
        NodeOutput Output;

        HSLProcessor processor;

        protected float hue;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Hue")]
        public float Hue
        {
            get
            {
                return hue;
            }
            set
            {
                hue = value;
                TriggerValueChange();
            }
        }

        protected float saturation;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Saturation", "Default", -1, 1)]
        public float Saturation
        {
            get
            {
                return saturation;
            }
            set
            {
                saturation = value;
                TriggerValueChange();
            }
        }

        protected float lightness;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Lightness", "Default", -1, 1)]
        public float Lightness
        {
            get
            {
                return lightness;
            }
            set
            {
                lightness = value;
                TriggerValueChange();
            }
        }

        public HSLNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "HSL";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new HSLProcessor();

            hue = 0;
            saturation = 0;
            lightness = 0;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Image");
            Output = new NodeOutput(NodeType.Color, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (processor != null)
            {
                processor.Release();
                processor = null;
            }
        }


        private void GetParams()
        {
            if (!input.HasInput) return;

            h = hue;
            s = saturation;
            l = lightness;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Hue"))
            {
                h = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Hue"));
            }
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Saturation"))
            {
                s = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Saturation"));
            }
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Lightness"))
            {
                l = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Lightness"));
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float h;
        float s;
        float l;
        void Process()
        {
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (processor == null) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;

            processor.Hue = h * 6.0f;
            processor.Saturation = s;
            processor.Lightness = l;

            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class HSLData : NodeData
        {
            public float hue;
            public float saturation;
            public float lightness;
        }

        public override void FromJson(string data)
        {
            HSLData d = JsonConvert.DeserializeObject<HSLData>(data);
            SetBaseNodeDate(d);
            hue = d.hue;
            saturation = d.saturation;
            lightness = d.lightness;
        }

        public override string GetJson()
        {
            HSLData d = new HSLData();
            FillBaseNodeData(d);
            d.hue = hue;
            d.saturation = saturation;
            d.lightness = lightness;

            return JsonConvert.SerializeObject(d);
        }
    }
}
