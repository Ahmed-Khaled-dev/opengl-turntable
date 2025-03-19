using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using GlmNet;
using System.IO;
using System.CodeDom;
using System.Diagnostics;

namespace Graphics
{
    class Renderer
    {
        Shader sh;

        Stopwatch timer = Stopwatch.StartNew();

        uint baseVertexBufferID;
        uint baseIndexBufferID;
        uint vinylVertexBufferID;
        uint bordersVertexBufferID;
        uint vinylCurveVertexBufferID;
        uint vinylLabelVertexBufferID;

        //3D Drawing
        mat4 GlobalModelMatrix;
        mat4 vinylCurveModelMatrix;
        mat4 ViewMatrix;
        mat4 ProjectionMatrix;

        int ShaderModelMatrixID;
        int ShaderViewMatrixID;
        int ShaderProjectionMatrixID;

        public bool vinylSpinning = true;

        Texture vinylLabel;

        public void Initialize()
        {
            timer.Start();
            string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            sh = new Shader(projectPath + "\\Shaders\\SimpleVertexShader.vertexshader", projectPath + "\\Shaders\\SimpleFragmentShader.fragmentshader");
            Gl.glClearColor(0.9f, 0.9f, 0.88f, 1);

            const float baseWidth = 1.5f;
            const float baseHeight = 0.7f;
            const float baseLength = 1.5f;
            const float baseRed = 0.545f;
            const float baseGreen = 0.271f;
            const float baseBlue = 0.075f;

            float[] baseVerts = { 
                // Turntable Cuboid Base
                // Positive X Face
		        baseWidth, baseHeight, baseLength, baseRed, baseGreen, baseBlue,
                baseWidth, baseHeight, -baseLength, baseRed, baseGreen, baseBlue,
                baseWidth, -baseHeight, baseLength, baseRed, baseGreen, baseBlue,
                baseWidth, -baseHeight, -baseLength, baseRed, baseGreen, baseBlue,

                // Negative Y Face
                -baseWidth, -baseHeight, baseLength, baseRed, baseGreen, baseBlue,
                -baseWidth, -baseHeight, -baseLength, baseRed, baseGreen, baseBlue,

                // Positive Y Face
                -baseWidth, baseHeight, baseLength, baseRed, baseGreen, baseBlue,
                -baseWidth, baseHeight, -baseLength, baseRed, baseGreen, baseBlue,
            };

            baseVertexBufferID = GPU.GenerateBuffer(baseVerts);

            ushort[] baseIndices = { 
                // Front Face
                0, 2, 4,   4, 6, 0,
                // Back Face
                1, 5, 3,   5, 3, 7,
                // Left Face
                4, 5, 6,   6, 7, 5,
                // Right Face
                0, 1, 2,   2, 3, 1,
                // Top Face
                0, 1, 6,   6, 7, 1,
                // Bottom Face
                2, 3, 4,   4, 5, 3
            };

            baseIndexBufferID = GPU.GenerateElementBuffer(baseIndices);

            const float vinylRadius = 1.35f;
            const int vinylSegments = 50;
            const float vinylHeight = baseHeight + 0.2f;
            const float vinylBlack = 0f;

            List<float> vinylVerts = new List<float>();

            // Origin point of the vinyl
            vinylVerts.Add(0f);  // X
            vinylVerts.Add(vinylHeight);  // Y
            vinylVerts.Add(0f); // Z
            vinylVerts.Add(vinylBlack);  // R
            vinylVerts.Add(vinylBlack);  // G
            vinylVerts.Add(vinylBlack);  // B

            for (int i = 0; i <= vinylSegments; i++)
            {
                float angle = (float)(2 * Math.PI * i / vinylSegments); // Convert index to angle
                float x = vinylRadius * (float) Math.Cos(angle); // X coordinate
                float z = vinylRadius * (float) Math.Sin(angle); // Y coordinate

                vinylVerts.Add(x);
                vinylVerts.Add(vinylHeight);
                vinylVerts.Add(z);
                vinylVerts.Add(vinylBlack); // R
                vinylVerts.Add(vinylBlack); // G
                vinylVerts.Add(vinylBlack); // B
            }

            vinylVertexBufferID = GPU.GenerateBuffer(vinylVerts.ToArray());

            const float borderRed = 0f;
            const float borderGreen = 0f;
            const float borderBlue = 0f;

            float[] baseBorderVerts = {
                baseWidth - 0.3f, baseHeight, baseLength - 0.3f, borderRed, borderGreen, borderBlue,
               -baseWidth + 0.3f, baseHeight, baseLength - 0.3f, borderRed, borderGreen, borderBlue,
               -baseWidth + 0.3f, baseHeight, -baseLength + 0.3f, borderRed, borderGreen, borderBlue,
                baseWidth - 0.3f, baseHeight, -baseLength + 0.3f, borderRed, borderGreen, borderBlue,

                -baseWidth, baseHeight - 1f, baseLength, borderRed, borderGreen, borderBlue,
                baseWidth, baseHeight - 1f, baseLength, borderRed, borderGreen, borderBlue,
                baseWidth, baseHeight - 1f, -baseLength, borderRed, borderGreen, borderBlue,

                -baseWidth, baseHeight, baseLength, borderRed, borderGreen, borderBlue,
                baseWidth, baseHeight, baseLength, borderRed, borderGreen, borderBlue,
                baseWidth, baseHeight, -baseLength, borderRed, borderGreen, borderBlue,
            };

            bordersVertexBufferID = GPU.GenerateBuffer(baseBorderVerts);

            const float vinylCurveRadius = vinylRadius * 0.8f;
            const int vinylCurveSegments = 10;

            List<float> vinylCurveVerts = new List<float>();

            for (int i = 0; i <= vinylCurveSegments; i++)
            {
                float angle = (float)((Math.PI / 12) * i / vinylCurveSegments); // Convert index to angle
                float x = vinylCurveRadius * (float)Math.Cos(angle); // X coordinate
                float z = vinylCurveRadius * (float)Math.Sin(angle); // Y coordinate

                vinylCurveVerts.Add(x);
                vinylCurveVerts.Add(vinylHeight);
                vinylCurveVerts.Add(z);
                vinylCurveVerts.Add(1.0f); // R
                vinylCurveVerts.Add(1.0f); // G
                vinylCurveVerts.Add(1.0f); // B
            }

            vinylCurveVertexBufferID = GPU.GenerateBuffer(vinylCurveVerts.ToArray());

            string vinylLabelName = "the-strokes.png";
            vinylLabel = new Texture(projectPath + "\\Textures\\" + vinylLabelName, 1);

            const float vinylLabelDistFromTopFace = 1.05f;

            float[] vinylLabelVerts = {
                baseWidth - vinylLabelDistFromTopFace, vinylHeight, baseLength - vinylLabelDistFromTopFace,
                1,0,
                baseWidth - vinylLabelDistFromTopFace, vinylHeight, -baseLength + vinylLabelDistFromTopFace,
                0,0,
                -baseWidth + vinylLabelDistFromTopFace, vinylHeight, baseLength - vinylLabelDistFromTopFace,
                1,1,

                -baseWidth + vinylLabelDistFromTopFace, vinylHeight, baseLength - vinylLabelDistFromTopFace,
                1,1,
                -baseWidth + vinylLabelDistFromTopFace, vinylHeight, -baseLength + vinylLabelDistFromTopFace,
                0,1,
                baseWidth - vinylLabelDistFromTopFace, vinylHeight, -baseLength + vinylLabelDistFromTopFace,
                0,0
            };

            vinylLabelVertexBufferID = GPU.GenerateBuffer(vinylLabelVerts);

            GlobalModelMatrix = new mat4(1);

            ViewMatrix = glm.lookAt(
                new vec3(4.5f, 4.5f, 2f),  // camera position
                new vec3(0, 0, 0),  // camera focus
                new vec3(0, 1, 0)); // camera rotation

            // glm.perspective(field of view, Width / Height, Near, Far);
            ProjectionMatrix = glm.perspective(45, 1, 0.1f, 100);

            sh.UseShader();

            ShaderModelMatrixID = Gl.glGetUniformLocation(sh.ID, "modelMatrix");
            ShaderViewMatrixID = Gl.glGetUniformLocation(sh.ID, "viewMatrix");
            ShaderProjectionMatrixID = Gl.glGetUniformLocation(sh.ID, "projectionMatrix");

            Gl.glUniformMatrix4fv(ShaderModelMatrixID, 1, Gl.GL_FALSE, GlobalModelMatrix.to_array());
            Gl.glUniformMatrix4fv(ShaderViewMatrixID, 1, Gl.GL_FALSE, ViewMatrix.to_array());
            Gl.glUniformMatrix4fv(ShaderProjectionMatrixID, 1, Gl.GL_FALSE, ProjectionMatrix.to_array());
        }

        public void Draw()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

            // Draw Base
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, baseVertexBufferID);
            Gl.glBindBuffer(Gl.GL_ELEMENT_ARRAY_BUFFER, baseIndexBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3*sizeof(float)));

            Gl.glUniformMatrix4fv(ShaderModelMatrixID, 1, Gl.GL_FALSE, GlobalModelMatrix.to_array());
            Gl.glDrawElements(Gl.GL_TRIANGLES, 36, Gl.GL_UNSIGNED_SHORT, (IntPtr)0);

            // Draw Black Borders On Base
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, bordersVertexBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_LINE_LOOP, 0, 4);
            Gl.glDrawArrays(Gl.GL_LINES, 4, 2);
            Gl.glDrawArrays(Gl.GL_LINES, 5, 2);
            Gl.glDrawArrays(Gl.GL_LINE_STRIP, 7, 3);

            // Draw Vinyl
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, vinylVertexBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_TRIANGLE_FAN, 0, 52);

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);

            // Draw White Curve On Vinyl
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, vinylCurveVertexBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 6 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glUniformMatrix4fv(ShaderModelMatrixID, 1, Gl.GL_FALSE, vinylCurveModelMatrix.to_array());
            Gl.glDrawArrays(Gl.GL_LINE_STRIP, 0, 11);

            // Draw Vinyl Label
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, vinylLabelVertexBufferID);

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 5 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(2);
            Gl.glVertexAttribPointer(2, 2, Gl.GL_FLOAT, Gl.GL_FALSE, 5 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);

            Gl.glDisableVertexAttribArray(0);
            Gl.glDisableVertexAttribArray(1);
            Gl.glDisableVertexAttribArray(2);
        }

        const float vinylCurveRotationSpeed = 1.6f;
        float vinylCurveRotationAngle = 0;

        public void Update()
        {
            timer.Stop();
            if (vinylSpinning)
            {
                float elapsedTime = timer.ElapsedMilliseconds / 1000.0f;

                vinylCurveRotationAngle += elapsedTime * vinylCurveRotationSpeed;
                vinylCurveModelMatrix = glm.rotate(vinylCurveRotationAngle, new vec3(0, 1, 0));
            }
            timer.Reset();
            timer.Start();
        }
        public void CleanUp()
        {
            sh.DestroyShader();
        }
    }
}
