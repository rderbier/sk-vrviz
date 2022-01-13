using System;
using System.Collections.Generic;
using StereoKit;


namespace RDR
{
   
    class SmartSphere
    {
        private static float gazeDuration = 1.5f; // 1.5 seconds
        public Pose pose;
        public Model model;
        public float scale;
        public string name;
        public Boolean isDetected;
        public Boolean isHandled;
        public Boolean isDraggable;
        public Boolean gazeIndicator = true;
        public Boolean isSelected; // true for the currently selected target
        public float gazeTime; // howlong this target has the gaze
        private static Mesh uvsphere;
        private Vec3 slidePosition;
        private Boolean trackRotationGesture = false;

        public static void Init()
        {
            // diameter, segements, rings
            SmartSphere.uvsphere = MeshUtils.generateUVSphere(1.0f, 60, 32);
          
        }

        public SmartSphere(Pose pose, Material material, float scale = 1.0f)
        {
            this.pose = new Pose(pose.position, pose.orientation);
            this.model = Model.FromMesh(
                uvsphere,
                material);

            this.scale = scale;
            this.name = (name != null) ? name : "target-" + Guid.NewGuid().ToString();
            this.isDraggable = true;

        }
       
        public Boolean Draw()
        {
            // draw the target and detect if the target is seen
            //

            isSelected = false;
         
            

            
            
                //UI.Label("Eye tracking active");
                // Intersect the eye Ray with the objects
                // user must be close enough
                if (Vec3.Distance(Input.Head.position, this.pose.position) < 1.0f)
                {
                    Sphere zone = new Sphere(this.pose.position, this.scale);
                    Ray view = new Ray(Input.Head.position, Input.Head.Forward);
                    if (view.Intersect(zone, out Vec3 at))
                    {
                        if (gazeIndicator)
                        {
                            Default.MeshSphere.Draw(Default.Material, Matrix.TS(at, .02f));
                        }
                        gazeTime += Time.Elapsedf;
                        if (gazeTime > gazeDuration)
                        {
                            isSelected = true;

                        }
                    }
                    else
                    {
                        gazeTime = 0f;
                    }
                }
            // handle touch rotation
            Hand hand = Input.Hand(Handed.Right);
            if (hand.IsTracked)
            {
                Vec3 fingertip = hand[FingerId.Index, JointId.Tip].position;
                // add a test on controller grip for oculus finger pointing !
                // Controller c = Input.Controller(Handed.Right); c.grip == 1 and c.trigger == 0 if finger point 
                if (Vec3.Distance(fingertip,this.pose.position) <= this.scale / 2.0f + 1*U.cm)
                {
                    if (trackRotationGesture == false)
                    { 
                        slidePosition = fingertip;
                        trackRotationGesture = true;
                    } else
                    {
                        if (Vec3.Distance(slidePosition,fingertip) > 1 *U.cm)
                        {
                            
                            Vec3 A = fingertip - this.pose.position;
                            Vec3 B = slidePosition - this.pose.position;
                            Vec3 axis = Vec3.Cross(B, A).Normalized;
                            
                            double cosalpha = Vec3.Dot(A,B) / (A.Magnitude * B.Magnitude);
                            double alpha = Math.Acos(cosalpha);
                            System.Numerics.Quaternion quat = System.Numerics.Quaternion.CreateFromAxisAngle(
                                new System.Numerics.Vector3(axis.x, axis.y, axis.z),
                                (float) alpha);
                            Quat rotation = new Quat(quat.X, quat.Y, quat.Z, quat.W);
                            // Matrix m = Matrix.R(rotation);
                            //this.pose.orientation = rotation * this.pose.orientation;
                            this.pose = new Pose(this.pose.position, rotation * this.pose.orientation);
                            // Log.Warn("rotate "+alpha);
                            slidePosition = fingertip;
                        }
                    }
                    Default.MeshSphere.Draw(Default.Material, Matrix.TS(fingertip, .02f));
                   
                } else
                {
                    if (trackRotationGesture == true)
                    {
                        Log.Warn("stop rotation");
                        trackRotationGesture = false;
                    }
                }
            } else
            {
                if (trackRotationGesture == true)
                {
                    Log.Warn("hand is not tracked");
                    Log.Warn("stop rotation");
                    trackRotationGesture = false;
                }
                
            }
            if (isSelected) { 
                }

            // set target Material
            //this.model.RootNode.Material = mat;
            // draw target
            Bounds scaledBounds = new Bounds(this.model.Bounds.center, this.model.Bounds.dimensions * scale);
            if (isDraggable)
            {
                this.isHandled = UI.Handle(this.name, ref this.pose, scaledBounds);
            } else
                {
                    this.isHandled = false;
                }
            Matrix targetTransform = this.pose.ToMatrix(scale); // move and scale
            this.model.Draw(targetTransform);
            return this.isHandled;
        }
        
 
    }
    
}