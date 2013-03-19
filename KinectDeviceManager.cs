using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;

namespace Deneme1
{
    class KinectDeviceManager
    {
        // Kinect Variables
        KinectSensor sensor;
        Texture2D colorVideo, depthVideo;
        Skeleton[] skeletonData;
        Skeleton skeleton;
        GraphicsDeviceManager graphics;

        public KinectDeviceManager(GraphicsDeviceManager _graphics)
        {
            this.graphics = _graphics;  
        }

        // Load Kinect Device
        public void LoadKinect()
        {
            // Connected kinect device count
            if (KinectSensor.KinectSensors.Count > 0)
            {
                if (KinectSensor.KinectSensors.Count == 1)
                {
                    this.sensor = KinectSensor.KinectSensors[0];
                }
            }
            else
            {
                Console.WriteLine("Connected Kinect Not Found...");
            }
        }
        
        // Start Loaded Kinect
        public void StartKinect()
        {
            if (this.sensor != null && !sensor.IsRunning)
            {
                this.sensor.Start();
                
            }
        }
        
        // Stop Kinect Device
        public void StopKinect()
        {
            if (this.sensor != null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }
        
        // Returns current sensor
        public KinectSensor GetSensor()
        {
            return this.sensor;
        }
        
        // Enables color, depth and skeleton streams
        public void EnableStreams()
        {
            // TODO parametre gec
            this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            this.sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            this.sensor.SkeletonStream.Enable();
        }
        
        // Add event handler for all frames
        public void AddEventForAllFrames()
        {
            this.sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(AllFramesReady);
        }
       
        // All Frame Event Handler
        public void AllFramesReady(object sender, AllFramesReadyEventArgs imageFrames)
        {
            //
            // Color Frame 
            //

            //Get raw image
            ColorImageFrame colorVideoFrame = imageFrames.OpenColorImageFrame();

            if (colorVideoFrame != null)
            {
                //Create array for pixel data and copy it from the image frame
                Byte[] pixelData = new Byte[colorVideoFrame.PixelDataLength];
                colorVideoFrame.CopyPixelDataTo(pixelData);

                //Convert RGBA to BGRA
                Byte[] bgraPixelData = new Byte[colorVideoFrame.PixelDataLength];
                for (int i = 0; i < pixelData.Length; i += 4)
                {
                    bgraPixelData[i] = pixelData[i + 2];
                    bgraPixelData[i + 1] = pixelData[i + 1];
                    bgraPixelData[i + 2] = pixelData[i];
                    bgraPixelData[i + 3] = (Byte)255; //The video comes with 0 alpha so it is transparent
                }

                // Create a texture and assign the realigned pixels
                colorVideo = new Texture2D(graphics.GraphicsDevice, colorVideoFrame.Width, colorVideoFrame.Height);
                colorVideo.SetData(bgraPixelData);
            }

            //
            // Depth Frame
            //
            DepthImageFrame depthVideoFrame = imageFrames.OpenDepthImageFrame();

            if (depthVideoFrame != null)
            {
                // Debug.WriteLineIf(debugging, "Frame");
                //Create array for pixel data and copy it from the image frame
                short[] pixelData = new short[depthVideoFrame.PixelDataLength];
                depthVideoFrame.CopyPixelDataTo(pixelData);

                //for (int i = 0; i < 10; i++)
                // { Debug.WriteLineIf(debugging, pixelData[i]); }

                // Convert the Depth Frame
                // Create a texture and assign the realigned pixels
                //
                depthVideo = new Texture2D(graphics.GraphicsDevice, depthVideoFrame.Width, depthVideoFrame.Height);
                depthVideo.SetData(ConvertDepthFrame(pixelData, this.sensor.DepthStream));

            }

            //
            // Skeleton Frame
            //
            using (SkeletonFrame skeletonFrame = imageFrames.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    //Copy the skeleton data to our array
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                }
            }

            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        skeleton = skel;
                    }
                }
            }
        }
        
        // Draw Skeleton
        public void DrawSkeleton(SpriteBatch spriteBatch, Vector2 resolution, Texture2D img)
        {
            if (skeleton != null)
            {
                foreach (Joint joint in skeleton.Joints)
                {
                    Vector2 position = new Vector2((((0.5f * joint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * joint.Position.Y) + 0.5f) * (resolution.Y)));
                    spriteBatch.Draw(img, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.Red);
                }
            }
        }
        
        // Conver to Depth frame
        private byte[] ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream)
        {
            int RedIndex = 0, GreenIndex = 1, BlueIndex = 2, AlphaIndex = 3;

            byte[] depthFrame32 = new byte[depthStream.FrameWidth * depthStream.FrameHeight * 4];

            for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < depthFrame32.Length; i16++, i32 += 4)
            {
                int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(~(realDepth >> 4));

                depthFrame32[i32 + RedIndex] = (byte)(intensity);
                depthFrame32[i32 + GreenIndex] = (byte)(intensity);
                depthFrame32[i32 + BlueIndex] = (byte)(intensity);
                depthFrame32[i32 + AlphaIndex] = 255;
            }

            return depthFrame32;
        }
        
        // Calculates angle between two vectors
        private double CalculateAngleBetween(Joint joint1, Joint jointMid, Joint joint2)
        {
            Vector3 vector1 = new Vector3();
            vector1.X = joint1.Position.X - jointMid.Position.X;
            vector1.Y = joint1.Position.Y - jointMid.Position.Y;
            vector1.Z = joint1.Position.Z - jointMid.Position.Z;

            Vector3 vector2 = new Vector3();
            vector2.X = joint2.Position.X - jointMid.Position.X;
            vector2.Y = joint2.Position.Y - jointMid.Position.Y;
            vector2.Z = joint2.Position.Z - jointMid.Position.Z;

            return Math.Atan2(vector2.Y - vector1.Y, vector2.X - vector1.X);
        }
    }
}
