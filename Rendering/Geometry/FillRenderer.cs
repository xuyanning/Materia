﻿using Materia.Rendering.Buffers;
using Materia.Rendering.Geometry;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Materia.Rendering.Geometry
{
    public class FillRenderer : IGeometry
    {
        public Fill FillData { get; protected set; }

        protected static GLTexture2D sharedGradientTex;
        protected static GLVertexArray sharedVao;
        protected GLArrayBuffer vbo;
        protected GLElementBuffer ebo;
        protected int indicesCount;

        protected GLTexture2D gradientTexture;
        public GLTexture2D GradientTexture
        {
            get
            {
                return gradientTexture;
            }
        }

        public FillRenderer(Fill d)
        {
            FillData = d;

            vbo = new GLArrayBuffer(Materia.Rendering.Interfaces.BufferUsageHint.StaticDraw);
            ebo = new GLElementBuffer(Materia.Rendering.Interfaces.BufferUsageHint.StaticDraw);
            gradientTexture = new GLTexture2D(PixelInternalFormat.Rgba8);
            UpdateGradientTexture();
        }

        public static GLVertexArray SharedVao
        {
            get
            {
                if (sharedVao == null)
                {
                    sharedVao = new GLVertexArray();
                }

                return sharedVao;
            }
        }

        public void Update()
        {
            if (vbo == null || ebo == null || FillData == null) return;

            vbo.Bind();
            ebo.Bind();

            try
            {

                if (FillData != null && FillData.Triangles != null 
                    && FillData.Triangles.Count > 0 && vbo.Id != 0 && ebo.Id != 0
                    && FillData.Points != null && FillData.Points.Count >= 3)
                {
                    int[] tris = FillData.Triangles.ToArray();
                    float[] data = FillData.Compact();
                    vbo.SetData(data);
                    ebo.SetData(tris);
                    indicesCount = tris.Length;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            GLArrayBuffer.Unbind();
            GLElementBuffer.Unbind();
        }

        public void UpdateGradientTexture()
        {
            //need to move this somewhere else eventually
            //so it will only update if we change color etc
            gradientTexture.Bind();
            gradientTexture.SetData(FillData.GradientMap.Image, PixelFormat.Bgra, FillData.GradientMap.Width, FillData.GradientMap.Height, 0);
            gradientTexture.Linear();
            gradientTexture.Repeat();
            GLTexture2D.Unbind();
        }

        public static void BindSharedVao()
        {
            SharedVao.Bind();
        }

        public static void UnbindSharedVao()
        {
            GLVertexArray.Unbind();
        }

        public static void DisposeSharedResources()
        {
            if (sharedVao != null)
            {
                sharedVao.Dispose();
                sharedVao = null;
            }
        }

        public void Dispose()
        {
            if (vbo != null)
            {
                vbo.Dispose();
                vbo = null;
            }

            if (ebo != null)
            {
                ebo.Dispose();
                ebo = null;
            }

            if (gradientTexture != null)
            {
                gradientTexture.Dispose();
                gradientTexture = null;
            }
        }

        public void Draw()
        {
            if (vbo == null || ebo == null || FillData == null 
                || indicesCount == 0 || FillData.Points == null || FillData.Points.Count < 3) return;

            vbo.Bind();
            ebo.Bind();

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
            gradientTexture?.Bind();

            IGL.Primary.VertexAttribPointer(0, 2, (int)VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            IGL.Primary.VertexAttribPointer(1, 2, (int)VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * 4);
            IGL.Primary.EnableVertexAttribArray(0);
            IGL.Primary.EnableVertexAttribArray(1);

            IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);

            GLArrayBuffer.Unbind();
            GLElementBuffer.Unbind();
            GLTexture2D.Unbind();
        }
    }
}