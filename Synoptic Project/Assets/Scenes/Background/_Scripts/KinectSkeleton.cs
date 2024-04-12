using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using System;

public class SkeletonGameObject {
    public GameObject root;
    public GameObject[] children;
    public float lastUpdateTime;
    public Vector3 lastPosition;
}

public class KinectSkeleton : MonoBehaviour {
    KinectController kinectController;

    List<SkeletonGameObject> skeletons = new List<SkeletonGameObject> ();
    // List<GameObject[]> skeletons = new List<GameObject[]> ();
    // Dictionary<GameObject, float> lastSkeletonActiveTimes;
    const float timeSkeletonCanBeDeactiveBeforeDelete = 3;
    const float maxDistanceToAssumeSamePersonReentry = 2;

    void Awake () {
        kinectController = FindObjectOfType<KinectController> ();
        if (kinectController == null) {
            print ("requires a kinect controller");
            return;
        }
    }

    /*
        Update checks for any skeletons?
        Found a skeleton! Checks if we have a real skeleton whos name is bodyID?
            Yes: Update that skeletons joints
            No: Check if we have any recently deactivated skeletons that are close in position to this new one (maybe its the same?)
    */

    void Update () {
        if (kinectController == null) return;

        List<SkeletonGameObject> toDelete = new List<SkeletonGameObject> ();
        // Queue the expired skeletons for deletion - user probably walked away
        foreach (SkeletonGameObject skeleton in skeletons) {
            if (Time.time > skeleton.lastUpdateTime + timeSkeletonCanBeDeactiveBeforeDelete) {
                print("deleting skeleton " + skeleton.root.name);
                toDelete.Add(skeleton);
            }
        }

        // Do the actual delete
        foreach (SkeletonGameObject skeleton in toDelete) {
            GameObject.Destroy (skeleton.root);
            skeletons.Remove (skeleton);
        }
        toDelete.Clear ();

        lock (kinectController.m_bufferLock) {
            List<SkeletonInfo> skeles = kinectController.m_currentSkeletons;
            foreach(SkeletonInfo sk in skeles) {
          
                uint bodyId = sk.id;

                // Check if skeleton already exists with same ID
                SkeletonGameObject existingSkeleton = skeletons.FirstOrDefault (skeleton => skeleton.root?.name == bodyId.ToString ());
                SkeletonGameObject existingextraSkeleton = skeletons.FirstOrDefault(skeleton => skeleton.root?.name == bodyId.ToString() + "extra");

                // Recognises this skeleton
                if (existingSkeleton != null) {
                    ApplyJointDataToSkeleton (sk.skeleton, existingSkeleton);
                    ApplyExtraJointDataToSkeleton (sk.skeleton, existingextraSkeleton, existingSkeleton);
                } else { // Unidentified skeleton
                    // Is there a recently disappeared one that was close to this new one? ie the same person?
                    foreach (SkeletonGameObject skeleton in skeletons) {
                        if (Time.time > skeleton.lastUpdateTime + .5f) {
                            if (Vector3.Distance (skeleton.lastPosition,
                                    new Vector3 (sk.skeleton.GetJoint(0).Position.X,
                                        sk.skeleton.GetJoint(0).Position.Y,
                                        sk.skeleton.GetJoint(0).Position.Z)) < maxDistanceToAssumeSamePersonReentry) {
                                skeleton.root.name = bodyId.ToString ();
                                return;
                            }
                        }
                    }
                    // Else it must really be a new person
                    CreateDebugSkeletons (bodyId.ToString ());
                    CreateExtraSkeletons(bodyId.ToString() + "extra");
                }
            }
        }
    }

    void ApplyJointDataToSkeleton (Skeleton skeletonData, SkeletonGameObject realSkeleton) {
        // Do joint moves
        for (var i = 0; i <(int)JointId.Count; i++) {
            var joint = skeletonData.GetJoint(i);
            var pos = joint.Position;
            var rot = joint.Quaternion;
            var v = new Vector3 (pos.X, -pos.Y, pos.Z) * 0.001f;
            var r = new Quaternion (rot.X, rot.Y, rot.Z, rot.W);
            realSkeleton.children[i].transform.localPosition = v;
            realSkeleton.children[i].transform.localRotation = r;
        }
        realSkeleton.lastUpdateTime = Time.time;
        realSkeleton.lastPosition = new Vector3 (skeletonData.GetJoint(0).Position.X, skeletonData.GetJoint(0).Position.Y, skeletonData.GetJoint(0).Position.Z);

        UpdateColour(realSkeleton);
        UpdateSize(realSkeleton, true);
    }

    void ApplyExtraJointDataToSkeleton(Skeleton skeletonData, SkeletonGameObject realSkeleton, SkeletonGameObject baseSkeleton)
    {
        // Do joint moves

        var prevJoint = skeletonData.GetJoint(0);
        int prevJointInd = 0;
        var nextJoint = skeletonData.GetJoint(0);
        int nextJointInd = 0;

        for (var i = 0; i < realSkeleton.children.Length; i++)
        {
            var joint = realSkeleton.children[i];
            string[] jointNames = joint.name.Split('-');
            jointNames[1] = jointNames[1].Substring(0, jointNames[1].Length - 1);

            for (var j = 0; j < (int)JointId.Count; j++)
            {
                if (System.Enum.ToObject(typeof(JointId), j).ToString() == jointNames[0])
                {
                    prevJoint = skeletonData.GetJoint(j);
                    prevJointInd = j;
                }
                if (System.Enum.ToObject(typeof(JointId), j).ToString() == jointNames[1])
                {
                    nextJoint = skeletonData.GetJoint(j);
                    nextJointInd = j;
                }
            }

            int count = 1;
            for (var j = 0; j < realSkeleton.children.Length; j++)
            {
                if (realSkeleton.children[j].name.Contains(baseSkeleton.children[prevJointInd].name) && realSkeleton.children[j].name.Contains(baseSkeleton.children[nextJointInd].name))
                {
                    count++;
                }
            }

            var pos = prevJoint.Position + (nextJoint.Position - prevJoint.Position) * Int32.Parse(joint.name[joint.name.Length - 1].ToString()) / (count);
            var rot = prevJoint.Quaternion;
            var v = new Vector3(pos.X, -pos.Y, pos.Z) * 0.001f;
            var r = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
            realSkeleton.children[i].transform.localPosition = v;
            realSkeleton.children[i].transform.localRotation = r;
        }
        realSkeleton.lastUpdateTime = Time.time;
        realSkeleton.lastPosition = new Vector3(skeletonData.GetJoint(0).Position.X, skeletonData.GetJoint(0).Position.Y, skeletonData.GetJoint(0).Position.Z);
    }

    void CreateDebugSkeletons (string rootName) {
        SkeletonGameObject newSkeleton = new SkeletonGameObject ();
        GameObject[] joints = new GameObject[(int)JointId.Count];
        GameObject skeletonRoot = new GameObject (rootName);
        for (int joint = 0; joint < (int)JointId.Count; joint++) {
            var cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(1f, 1f, 1f);
            cube.name = System.Enum.ToObject(typeof(JointId), joint).ToString();
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = Vector3.one * 0.05f;
            cube.transform.parent = skeletonRoot.transform;
            joints[joint] = cube;
        }
        newSkeleton.root = skeletonRoot;
        newSkeleton.children = joints;
        newSkeleton.lastUpdateTime = Time.time;

        skeletons.Add (newSkeleton);
        print ("created a skeleton " + skeletonRoot.name);
    }

    void CreateExtraSkeletons (string rootName)
    {
        SkeletonGameObject newSkeleton = new SkeletonGameObject();
        //GameObject[] joints = new GameObject[(int)JointId.Count];
        GameObject[] joints = new GameObject[65];
        GameObject skeletonRoot = new GameObject(rootName);
        for (int joint = 0; joint < joints.Length; joint++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (joint == 0)
            {
                cube.name = "Pelvis-SpineNavel1";
            }
            else if (joint == 1)
            {
                cube.name = "Pelvis-SpineNavel2";
            }
            else if (joint == 2)
            {
                cube.name = "SpineNavel-SpineChest1";
            }
            else if (joint == 3)
            {
                cube.name = "SpineChest-Neck1";
            }
            else if (joint == 4)
            {
                cube.name = "SpineChest-Neck2";
            }
            else if (joint == 5)
            {
                cube.name = "Neck-ClavicleLeft1";
            }
            else if (joint == 6)
            {
                cube.name = "ClavicleLeft-ShoulderLeft1";
            }
            else if (joint == 7)
            {
                cube.name = "ShoulderLeft-ElbowLeft1";
            }
            else if (joint == 8)
            {
                cube.name = "ShoulderLeft-ElbowLeft2";
            }
            else if (joint == 9)
            {
                cube.name = "ShoulderLeft-ElbowLeft3";
            }
            else if (joint == 10)
            {
                cube.name = "ElbowLeft-WristLeft1";
            }
            else if (joint == 11)
            {
                cube.name = "ElbowLeft-WristLeft2";
            }
            else if (joint == 12)
            {
                cube.name = "WristLeft-HandLeft1";
            }
            else if (joint == 13)
            {
                cube.name = "HandLeft-HandTipLeft1";
            }
            else if (joint == 14)
            {
                cube.name = "HandLeft-ThumbLeft1";
            }
            else if (joint == 15)
            {
                cube.name = "Neck-ClavicleRight1";
            }
            else if (joint == 16)
            {
                cube.name = "ClavicleRight-ShoulderRight1";
            }
            else if (joint == 17)
            {
                cube.name = "ShoulderRight-ElbowRight1";
            }
            else if (joint == 18)
            {
                cube.name = "ShoulderRight-ElbowRight2";
            }
            else if (joint == 19)
            {
                cube.name = "ShoulderRight-ElbowRight3";
            }
            else if (joint == 20)
            {
                cube.name = "ElbowRight-WristRight1";
            }
            else if (joint == 21)
            {
                cube.name = "ElbowRight-WristRight2";
            }
            else if (joint == 22)
            {
                cube.name = "WristRight-HandRight1";
            }
            else if (joint == 23)
            {
                cube.name = "HandRight-HandTipRight1";
            }
            else if (joint == 24)
            {
                cube.name = "HandRight-ThumbRight1";
            }
            else if (joint == 25)
            {
                cube.name = "Pelvis-HipLeft1";
            }
            else if (joint == 26)
            {
                cube.name = "HipLeft-KneeLeft1";
            }
            else if (joint == 27)
            {
                cube.name = "HipLeft-KneeLeft2";
            }
            else if (joint == 28)
            {
                cube.name = "HipLeft-KneeLeft3";
            }
            else if (joint == 29)
            {
                cube.name = "KneeLeft-AnkleLeft1";
            }
            else if (joint == 30)
            {
                cube.name = "KneeLeft-AnkleLeft2";
            }
            else if (joint == 31)
            {
                cube.name = "AnkleLeft-FootLeft1";
            }
            else if (joint == 32)
            {
                cube.name = "Pelvis-HipRight1";
            }
            else if (joint == 33)
            {
                cube.name = "HipRight-KneeRight1";
            }
            else if (joint == 34)
            {
                cube.name = "HipRight-KneeRight2";
            }
            else if (joint == 35)
            {
                cube.name = "HipRight-KneeRight3";
            }
            else if (joint == 36)
            {
                cube.name = "KneeRight-AnkleRight1";
            }
            else if (joint == 37)
            {
                cube.name = "KneeRight-AnkleRight2";
            }
            else if (joint == 38)
            {
                cube.name = "AnkleRight-FootRight1";
            }
            else if (joint == 39)
            {
                cube.name = "Neck-Head1";
            }
            else if (joint == 40)
            {
                cube.name = "Head-Nose1";
            }
            else if (joint == 41)
            {
                cube.name = "Head-EyeLeft1";
            }
            else if (joint == 42)
            {
                cube.name = "Head-EarLeft1";
            }
            else if (joint == 43)
            {
                cube.name = "Head-EyeRight1";
            }
            else if (joint == 44)
            {
                cube.name = "Head-EarRight1";
            }
            else if (joint == 45)
            {
                cube.name = "EarLeft-EarRight1";
            }
            else if (joint == 46)
            {
                cube.name = "EarLeft-EarRight2";
            }
            else if (joint == 47)
            {
                cube.name = "EarLeft-EyeRight1";
            }
            else if (joint == 48)
            {
                cube.name = "EarRight-EyeLeft1";
            }
            else if (joint == 49)
            {
                cube.name = "ClavicleLeft-SpineChest1";
            }
            else if (joint == 50)
            {
                cube.name = "ClavicleLeft-SpineChest2";
            }
            else if (joint == 51)
            {
                cube.name = "ClavicleRight-SpineChest1";
            }
            else if (joint == 52)
            {
                cube.name = "ClavicleRight-SpineChest2";
            }
            else if (joint == 53)
            {
                cube.name = "ShoulderLeft-SpineNavel1";
            }
            else if (joint == 54)
            {
                cube.name = "ShoulderLeft-SpineNavel2";
            }
            else if (joint == 55)
            {
                cube.name = "ShoulderLeft-SpineNavel3";
            }
            else if (joint == 56)
            {
                cube.name = "ShoulderRight-SpineNavel1";
            }
            else if (joint == 57)
            {
                cube.name = "ShoulderRight-SpineNavel2";
            }
            else if (joint == 58)
            {
                cube.name = "ShoulderRight-SpineNavel3";
            }
            else if (joint == 59)
            {
                cube.name = "ShoulderLeft-SpineChest1";
            }
            else if (joint == 60)
            {
                cube.name = "ShoulderRight-SpineChest1";
            }
            else if (joint == 61)
            {
                cube.name = "HipLeft-SpineNavel1";
            }
            else if (joint == 62)
            {
                cube.name = "HipLeft-SpineNavel2";
            }
            else if (joint == 63)
            {
                cube.name = "HipRight-SpineNavel1";
            }
            else if (joint == 64)
            {
                cube.name = "HipRight-SpineNavel2";
            }
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = Vector3.one * 0.05f;
            cube.transform.parent = skeletonRoot.transform;
            joints[joint] = cube;
        }
        newSkeleton.root = skeletonRoot;
        newSkeleton.children = joints;
        newSkeleton.lastUpdateTime = Time.time;

        skeletons.Add(newSkeleton);
        print("created a skeleton " + skeletonRoot.name);
    }