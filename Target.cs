using System;
using System.Collections.Generic;
using StereoKit;


namespace RDR
{
    class TargetGroup
    {
        private List<Target> targetList;
        public Target currentTarget;
       
        public TargetGroup()
        {
            
            targetList = new List<Target>();
        }
        public void addTarget(Pose pose, float diameter, String name, Material targetMaterial, Material seenMaterial, Material selectedMaterial)
        {
            Model target = Model.FromMesh(
                //Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
                Mesh.GenerateSphere(1.0f),
                targetMaterial);
            Target t = new Target(pose, target, diameter, name, targetMaterial, seenMaterial, selectedMaterial);
            targetList.Add(t);
            setTargetPose(t, pose);
            currentTarget = t;
        }
        
        public void draw()
        {
            foreach (var target in this.targetList)
            {
                if (target.draw(0f))
                {
                    target.logClosest();
                    Log.Info("distance to line " + target.distanceToClosestLine(target.pose.position));
                    if (target.collide() == null)
                    {
                        this.setTargetPose(target,target.newPose);
                    }
                    
                }
               // if (target.isSelected)
                {
                    randomMove(target);
                }
                if (target.isHandled)
                {
                    Log.Info("target weight : "+target.positioningFunctionvalue);
                }
            }
        }
        public void setTargetPose(Target t, Pose newPose)
        {
            t.setPose(newPose);

            foreach (var target in this.targetList)
            {
                if (target != t)
                {
                    if (target.updateClosestList(t))
                    {
                        target.computePositioningFunction();
                    }
                    t.updateClosestList(target);

                }
            }

        }
        public Boolean randomMove(Target t)
        {
            Boolean hasMoved = false;
            Vec3 dir = Geometry.RandomDirection();
            Vec3 testedPosition = t.pose.position +  dir * 0.01f;
            if (t.isNewPositionBetter(testedPosition))
            {
                setTargetPose(t, new Pose(testedPosition,t.pose.orientation));
            }

            return hasMoved;
        }

    }
    class Target
    {
        public Pose pose, newPose;
        public Model model;
        public float scale;
        public string name;
        public Boolean isDetected;
        public Boolean isHandled;
        public Sound memo;
        public Boolean isSelected; // true for the currently selected target
        public float gazeTime; // howlong this target has the gaze
        private String anchorID;
        private Material targetMaterial, seenMaterial, selectedMaterial;
        public Target[] closestTargetArray;
        public float positioningFunctionvalue;


        public Target(Pose pose, Model target, float scale, String name, Material targetMaterial, Material seenMaterial, Material selectedMaterial)
        {
            this.pose = new Pose(pose.position, pose.orientation);
            this.model = target;
            this.scale = scale;
            this.seenMaterial = seenMaterial;
            this.targetMaterial = targetMaterial;
            this.selectedMaterial = selectedMaterial;

            this.name = (name != null) ? name : "target-" + Guid.NewGuid().ToString();
            
            this.gazeTime = 0f;
            closestTargetArray = new Target[2];


        }
        public void setPose(Pose newPose)
        {
            pose.position = newPose.position;
            pose.orientation = newPose.orientation;
            computePositioningFunction();
        }
        public Boolean updateClosestList(Target t)
        {
            Target targetToEvaluate = t;
            Boolean changed = false;
            int i = 0;
            while ((targetToEvaluate != null) && (i<this.closestTargetArray.Length)) 
            {
                if  (this.closestTargetArray[i] == null)
                {
                    this.closestTargetArray[i] = targetToEvaluate;
                    changed = true;
                    break;
                } else if (this.closestTargetArray[i] == t)
                {
                    changed = true;
                    break;
                } 
                else if ( Vec3.Distance(targetToEvaluate.pose.position, this.pose.position)  < Vec3.Distance(this.closestTargetArray[i].pose.position, this.pose.position)) 
                {
                    Target temp = this.closestTargetArray[i];
                    this.closestTargetArray[i] = targetToEvaluate;
                    changed = true;

                    // remove t if present after this index
                    for (int j = i+1; j < this.closestTargetArray.Length; j++)
                    {
                        if (closestTargetArray[j] == targetToEvaluate)
                        {
                            closestTargetArray[j] = null; // has moved upper
                        }
                    }
                    targetToEvaluate = temp;
                }
                i++;
            }
            return changed; 
        }
        public void logClosest()
        {
            for (int i = 0; i < this.closestTargetArray.Length; i++)
            {
                if (closestTargetArray[i] != null)  {
                   // Log.Info("Is close to ["+i+"] "+ closestTargetArray[i].name);
                    Lines.Add(this.pose.position, closestTargetArray[i].pose.position, new Color(1, 0, 0, 1), 0.01f);
                }
            }
        }
        public Target collide()
        {
            Target touch = null;

            // only test the closest target
            if ((closestTargetArray[0]!=null) &&
                (Vec3.Distance(newPose.position, closestTargetArray[0].pose.position) <= (this.scale + closestTargetArray[0].scale) / 2) ) {
                touch = closestTargetArray[0];
            }
            return touch;

        }
        public float distanceToClosestLine(Vec3 position)
        {
            float distance = float.MaxValue;
            if ((closestTargetArray[0]!=null) &&(closestTargetArray[1]!=null)) {

                Vec3 A = closestTargetArray[0].pose.position;
                Vec3 B = closestTargetArray[1].pose.position;

                Vec3 d = B - A; // vecteur directeur;
                distance = Vec3.Cross(position - A, d).Magnitude / d.Magnitude;
                


            }
            return distance;
        }
        public float ratioDistanceToClosest(Vec3 C)
        {
            float ratio = float.MaxValue;
            if ((closestTargetArray[0] != null) && (closestTargetArray[1] != null))
            {
                
                Vec3 A = closestTargetArray[0].pose.position;
                Vec3 B = closestTargetArray[1].pose.position;
                float d1, d2;
                if (Vec3.Dot(A-C,B-C) < 0)
                {
                    d1 = Vec3.Distance(C,A);
                    d2 = Vec3.Distance(C,B);
                } else if (Vec3.Dot(A-B, C-B) < 0)
                {
                    d1 = Vec3.Distance(B, A);
                    d2 = Vec3.Distance(C, C);
                } else
                {
                    d1 = Vec3.Distance(A,B);
                    d2 = Vec3.Distance(A,C);
                }
                  
                
                ratio = (d1 < d2) ? d2 / d1 : d1 / d2;
               
                

                
                if (ratio < 3.0f)
                {
                    ratio = Math.Abs(ratio - (float)Math.Round(ratio));
                }

                
            }
            return ratio;

        }
        
        public Boolean isNewPositionBetter(Vec3 position)
        {
            return (computePositioningFunction(position) < this.positioningFunctionvalue);

        }
        public void computePositioningFunction() {
           
            this.positioningFunctionvalue = this.computePositioningFunction(this.pose.position);
        }
        private float computePositioningFunction(Vec3 position)
        {
            float w = distanceToClosestLine(position);
            if (w < 0.01f)
            {
                w += ratioDistanceToClosest(position);
            }
            
            return w;

        }
        public Boolean draw(float gazeDuration,  Boolean isDraggable = true, float distance = 2.0f, Boolean gazeIndicator = false)
        {
            // draw the target and detect if the target is seen
            //

            isSelected = false;
            Material mat = this.targetMaterial;
            Matrix targetTransform = this.pose.ToMatrix(scale); // move and scale

            
            {
                //UI.Label("Eye tracking active");
                // Intersect the eye Ray with the objects
                // user must be close enough
                if (Vec3.Distance(Input.Head.position, this.pose.position) < distance)
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
            }
            if (isSelected) { mat = this.seenMaterial; }

            // set target Material
            this.model.RootNode.Material = mat;
            // draw target
            Bounds scaledBounds = new Bounds(this.model.Bounds.center, this.model.Bounds.dimensions * scale);
            if (isDraggable)
            {
                this.newPose = new Pose(pose.position, pose.orientation);
                this.isHandled = UI.Handle(this.name, ref this.newPose, scaledBounds);
            }
            this.model.Draw(targetTransform);
            return this.isHandled;
        }
        
 
    }
    
}