using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEngine;
using Unity.BlinkyShared.DMX;
using Unity.BlinkyLights;
using Unity.BlinkyBlinky;
using Unity.BlinkyNetworking;
//using Unity.BlinkyLights;

[System.Serializable]
public struct FixtureData
{
    public string _MappingFilename;
    public short _StartUniverse;
}

namespace Unity.BlinkyLights
{   
    /// <summary>
    /// A game objects that holds the fixture object and gives it a parent transform to update the LED positions
    /// </summary>
    public class Fixture : MonoBehaviour
    {
        public enum ControllerType
        {
            PixliteController,
            ArtnetController,
        }

        UKILightingManager _Manager;
        public Transform _TFormToFollow;
        public Vector2 _OriginalPositionOffset;

        public ControllerType _ControllerType = ControllerType.PixliteController;
        public FixtureData[] _FixtureData;

        //Fixture _Fixture;
        public List<LedChain> LedChains = new List<LedChain>();
        public DMXDeviceDetail DmxDevice;
        public string NetworkName => DmxDevice.NetworkName;

        public float _Smoothing = 0;

        public Bounds _Bounds;

        bool isDirty = false;


        Vector3 _Position;
        Vector3 Position
        {
            get { return _Position; }
            set
            {
                if (_Position != value)
                    isDirty = true;

                _Position = value;
            }
        }

        Vector3 _Rotation;
        Vector3 Rotation
        {
            get { return _Rotation; }
            set
            {
                if (_Rotation != value)
                    isDirty = true;

                _Rotation = value;
            }
        }

        float _Scale = 1;
        float Scale
        {
            get { return _Scale; }
            set
            {
                if (_Scale != value)
                    isDirty = true;

                _Scale = value;
            }
        }

        
        public DMXDatagram[] _Datagrams;

        public bool _DrawDebug = false;

        // Start is called before the first frame update
        public Fixture Init(UKILightingManager lightingManager)
        {
            _Manager = lightingManager;

            switch (_ControllerType)
            {
                case ControllerType.PixliteController:
                    DmxDevice = lightingManager.PIXLITE_CONTROLLER;
                    break;
                case ControllerType.ArtnetController:
                    DmxDevice = lightingManager.ARTNET_CONTROLLERS;
                    break;
            }

            for (int i = 0; i < _FixtureData.Length; i++)
            {
                string filePath = Application.streamingAssetsPath + "/FixtureMappings/" + _FixtureData[i]._MappingFilename;
                TryLoadLedChainFromFile(filePath, _FixtureData[i]._StartUniverse, _OriginalPositionOffset);
            }

            _Datagrams = DatagramComposer.InitDMXDatagrams(this);
            Debug.Log(name + "     Fixture initialized with datagram length: " + _Datagrams.Length);

            print("Fixture loaded: " + name);
            return this;
        }

        public void AddLedChain(LedChain chain)
        {
            LedChains.Add(chain);
        }

        // Update is called once per frame
        public void Update()
        {
            transform.position = _TFormToFollow.position;
            transform.rotation = _TFormToFollow.rotation;
            transform.localScale = _TFormToFollow.localScale;

            Position = transform.position;
            Rotation = transform.localEulerAngles;
            Scale = transform.localScale.x;

            //if (isDirty)
            {
                UpdateTransform(transform);
                isDirty = false;
            }
        }

        public DMXDatagram[] UpdateDatagrams()
        {
            _Datagrams = DatagramComposer.UpdateDMXDatagramBuffers(this, _Datagrams);
            return _Datagrams;
        }

        /// <summary>
        /// Attempt to load all the pixels based on an x,y index file. 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="universe"></param>
        public void TryLoadLedChainFromFile(string filePath, short universe, Vector2 offset)
        {
            try
            {
                //Name the chain based on the source index file
                var chain = new LedChain(Path.GetFileNameWithoutExtension(filePath), universe);


                //read Data Out of the file
                var csv = from line in File.ReadAllLines(filePath)
                          select line.Split(',');

                foreach (var led in csv)
                {
                    var x = (float.Parse(led[0]) * .001f) + offset.x;
                    var y = (float.Parse(led[1]) * .001f) + offset.y;

                    chain.AddPixel(new Pixel(x, y, _Smoothing));

                    _Bounds.Encapsulate(new Vector3(x, y));
                }

                AddLedChain(chain);
            }
            catch (Exception e)
            {
                Debug.Log("Loading an led mapping file fialed. " + e.ToString());
            }

        }


        /// <summary>If using consecutive strings for a fixture, this helper will tell you what the next universe for the next string is.
        /// </summary>
        public short GetNextUniverse()
        {
            if (!LedChains.Any()) return 0;
            return (short)(LedChains.Last().DMXStartingUniverse + LedChains.Last().DmxUniversesRequired);
        }

        public void UpdateTransform(Transform parent)
        {
            foreach (var ledChain in LedChains)
            {
                foreach (var pixel in ledChain.Pixels)
                {
                    pixel.UpdateLocation(parent, _Manager._InputTransform);// = parent.transform.position;// + parent.TransformPoint(pixel.originalLocation);
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (_DrawDebug && Application.isPlaying)
            {
                foreach (LedChain ledChain in LedChains)
                {
                    foreach (Pixel p in ledChain.Pixels)
                    {
                        //Gizmos.color = Color.blue;
                        Gizmos.color = p.Color / 255f;
                        Gizmos.DrawWireCube(p.currentLocation, _Manager._DebugGizmoScale);
                    }
                }
            }
        }
    }
}

