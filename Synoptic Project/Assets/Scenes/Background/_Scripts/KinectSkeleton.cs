using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;

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
        GameObject avatar = GameObject.Find("/Avatar1");
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
                print ("deleting skeleton " + skeleton.root.name);
                toDelete.Add (skeleton);
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

                // Recognises this skeleton
                if (existingSkeleton != null) {
                    ApplyJointDataToSkeleton (sk.skeleton, existingSkeleton);
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
                }
            }
        }
    }

    void ApplyJointDataToSkeleton (Skeleton skeletonData, SkeletonGameObject realSkeleton) {
        // Do joint moves
        //Debug.Log((int)JointId.Count);32
        var pelvisAngle = new Quaternion();
        var spinenavelAngle = new Quaternion();
        var spinechestAngle = new Quaternion();
        var neckAngle = new Quaternion();
        var leftclavicleAngle = new Quaternion();
        var leftshoulderAngle = new Quaternion();
        var leftelbowAngle = new Quaternion();
        var lefthandAngle = new Quaternion();

        for (var i = 0; i <(int)JointId.Count; i++) {
            var joint = skeletonData.GetJoint(i);
            var pos = joint.Position;
            var rot = joint.Quaternion;
            var v = new Vector3 (pos.X, -pos.Y, pos.Z) * 0.001f;
            var r = new Quaternion (rot.X, rot.Y, rot.Z, rot.W);
            realSkeleton.children[i].transform.localPosition = v;
            realSkeleton.children[i].transform.localRotation = r;

            if (i == 0)
            {
                GameObject pelvis = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips");
                pelvis.transform.localPosition = v;
                Quaternion r2 = Quaternion.identity;
                r2.eulerAngles = new Vector3(90, -90, 0);
                pelvis.transform.localRotation = r * r2;

                pelvisAngle = r * r2;
            }
            else if (i == 1)
            {
                GameObject spinenavel = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1");
                Quaternion r3 = Quaternion.identity;
                r3.eulerAngles = new Vector3(90, -90, 0) - pelvisAngle.eulerAngles;
                spinenavel.transform.localRotation = Quaternion.identity;
                spinenavel.transform.localRotation = r * r3;

                spinenavelAngle = r * r3;
            }
            else if (i == 2)
            {
                GameObject spinechest = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2");
                Quaternion r4 = Quaternion.identity;
                r4.eulerAngles = new Vector3(90, -90, 0) - spinenavelAngle.eulerAngles;
                spinechest.transform.localRotation = Quaternion.identity;
                spinechest.transform.localRotation = r * r4;

                spinechestAngle = r * r4;
            }
            else if (i == 3)
            {
                GameObject Neck = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_Neck");
                Quaternion r5 = Quaternion.identity;
                r5.eulerAngles = new Vector3(90, -90, 0) - spinechestAngle.eulerAngles;
                Neck.transform.localRotation = Quaternion.identity;
                Neck.transform.localRotation = r * r5;

                neckAngle = r * r5;
            }
            else if (i == 4)
            {
                GameObject ClavicleLeft = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_LeftShoulder");
                Quaternion r6 = Quaternion.identity;
                r6.eulerAngles = new Vector3(90, 0, 0) - spinechestAngle.eulerAngles;
                ClavicleLeft.transform.localRotation = Quaternion.identity;
                ClavicleLeft.transform.localRotation = r * r6;

                leftclavicleAngle = r * r6;
            }
            else if (i == 5)
            {
                GameObject ShoulderLeft = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_LeftShoulder/Character1_LeftArm");
                Quaternion r7 = Quaternion.identity;
                r7.eulerAngles = new Vector3(90, 0, 0) - leftclavicleAngle.eulerAngles;
                ShoulderLeft.transform.localRotation = Quaternion.identity;
                ShoulderLeft.transform.localRotation = r * r7;

                leftshoulderAngle = r * r7;
            }
            else if (i == 6)
            {
                GameObject ElbowLeft = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_LeftShoulder/Character1_LeftArm/Character1_LeftForeArm");
                Quaternion r8 = Quaternion.identity;
                r8.eulerAngles = new Vector3(90, 0, 0) - leftshoulderAngle.eulerAngles;
                ElbowLeft.transform.localRotation = Quaternion.identity;
                ElbowLeft.transform.localRotation = r * r8;

                leftelbowAngle = r * r8;
            }
            else if (i == 7)
            {
                GameObject WristLeft = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_LeftShoulder/Character1_LeftArm/Character1_LeftForeArm/Character1_LeftHand");
                Quaternion r9 = Quaternion.identity;
                r9.eulerAngles = new Vector3(-90, 0, 0) - leftshoulderAngle.eulerAngles;
                WristLeft.transform.localRotation = Quaternion.identity;
                WristLeft.transform.localRotation = r * r9;
            }
            //else if (i == 11)
            //{
            //    GameObject ClavicleRight = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_RightShoulder");
            //    Quaternion r13 = Quaternion.identity;
            //    r13.eulerAngles = new Vector3(-90, 0, 0);
            //    ClavicleRight.transform.localRotation = r * r13;
            //}
            //else if (i == 12)
            //{
            //    GameObject ShoulderRight = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_RightShoulder/Character1_RightArm");
            //    Quaternion r14 = Quaternion.identity;
            //    r14.eulerAngles = new Vector3(-90, 0, 0);
            //    ShoulderRight.transform.localRotation = r * r14;
            //}
            //else if (i == 13)
            //{
            //    GameObject ElbowRight = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_RightShoulder/Character1_RightArm/Character1_RightForeArm");
            //    Quaternion r15 = Quaternion.identity;
            //    r15.eulerAngles = new Vector3(-90, 0, 0);
            //    ElbowRight.transform.localRotation = r * r15;
            //}
            //else if (i == 14)
            //{
            //    GameObject WristRight = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_RightShoulder/Character1_RightArm/Character1_RightForeArm/Character1_RightHand");
            //    Quaternion r16 = Quaternion.identity;
            //    r16.eulerAngles = new Vector3(90, 0, 0);
            //    WristRight.transform.localRotation = r * r16;
            //}
            //else if (i == 18)
            //{
            //    GameObject HipLeft = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_LeftUpLeg");
            //    Quaternion r20 = Quaternion.identity;
            //    r20.eulerAngles = new Vector3(90, -90, 0);
            //    HipLeft.transform.localRotation = r * r20;
            //}
            //else if (i == 19)
            //{
            //    GameObject KneeLeft = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_LeftUpLeg/Character1_LeftLeg");
            //    Quaternion r21 = Quaternion.identity;
            //    r21.eulerAngles = new Vector3(90, -90, 0);
            //    KneeLeft.transform.localRotation = r * r21;
            //}
            //else if (i == 20)
            //{
            //    GameObject AnkleLeft = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_LeftUpLeg/Character1_LeftLeg/Character1_LeftFoot");
            //    Quaternion r22 = Quaternion.identity;
            //    r22.eulerAngles = new Vector3(90, -90, 0);
            //    AnkleLeft.transform.localRotation = r * r22;
            //}
            //else if (i == 21)
            //{
            //    GameObject FootLeft = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_LeftUpLeg/Character1_LeftLeg/Character1_LeftFoot/Character1_LeftToeBase");
            //    Quaternion r23 = Quaternion.identity;
            //    r23.eulerAngles = new Vector3(90, -90, 0);
            //    FootLeft.transform.localRotation = r * r23;
            //}
            //else if (i == 22)
            //{
            //    GameObject HipRight = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_RightUpLeg");
            //    Quaternion r24 = Quaternion.identity;
            //    r24.eulerAngles = new Vector3(-90, -90, 0);
            //    HipRight.transform.localRotation = r * r24;
            //}
            //else if (i == 23)
            //{
            //    GameObject KneeRight = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_RightUpLeg/Character1_RightLeg");
            //    Quaternion r25 = Quaternion.identity;
            //    r25.eulerAngles = new Vector3(-90, -90, 0);
            //    KneeRight.transform.localRotation = r * r25;
            //}
            //else if (i == 24)
            //{
            //    GameObject AnkleRight = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_RightUpLeg/Character1_RightLeg/Character1_RightFoot");
            //    Quaternion r26 = Quaternion.identity;
            //    r26.eulerAngles = new Vector3(-90, -90, 0);
            //    AnkleRight.transform.localRotation = r * r26;
            //}
            //else if (i == 25)
            //{
            //    GameObject FootRight = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_RightUpLeg/Character1_RightLeg/Character1_RightFoot/Character1_RightToeBase");
            //    Quaternion r27 = Quaternion.identity;
            //    r27.eulerAngles = new Vector3(-90, -90, 0);
            //    FootRight.transform.localRotation = r * r27;
            //}
            else if (i == 26)
            {
                GameObject Head = GameObject.Find("Avatar1/Character1_Reference/Character1_Hips/Character1_Spine/Character1_Spine1/Character1_Spine2/Character1_Neck/Character1_Head");
                Quaternion r28 = Quaternion.identity;
                r28.eulerAngles = new Vector3(90, -90, 0) - neckAngle.eulerAngles;
                Head.transform.localRotation = r * r28;
            }
        }
        realSkeleton.lastUpdateTime = Time.time;
        realSkeleton.lastPosition = new Vector3 (skeletonData.GetJoint(0).Position.X, skeletonData.GetJoint(0).Position.Y, skeletonData.GetJoint(0).Position.Z);
    }

    void CreateDebugSkeletons (string rootName) {
        SkeletonGameObject newSkeleton = new SkeletonGameObject ();
        GameObject[] joints = new GameObject[(int)JointId.Count];
        GameObject skeletonRoot = new GameObject (rootName);
        for (int joint = 0; joint < (int)JointId.Count; joint++) {
            var cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
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
}