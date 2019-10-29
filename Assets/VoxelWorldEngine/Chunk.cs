﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoxelWorldEngine.Enums;
using VoxelWorldEngine.Noise;

namespace VoxelWorldEngine
{
    public class Chunk : MonoBehaviour
    {
        public BlockType[,,] Blocks;
        public const int XSize = 16;
        public const int YSize = 256;
        public const int ZSize = 16;

        public ChunkState State { get; private set; }
        //*************************************************************
        private List<Vector3> m_vertices = new List<Vector3>();
        private List<int> m_triangles = new List<int>();
        private List<Color> m_colors = new List<Color>();
        private List<Vector2> m_uv = new List<Vector2>();
        private int m_faceCount = 0;

        private Mesh m_mesh;
        private MeshCollider m_collider;

        //TODO: implement the world class
        private WorldGenerator m_parent;

        Thread t;
        Vector3 mainpos;
        //*************************************************************
        #region Behaviour methods
        void Awake()
        {
            //Initialize the block array
            Blocks = new BlockType[XSize, YSize, ZSize];

            //Get the gameobject components
            m_mesh = this.GetComponent<MeshFilter>().mesh;
            m_collider = this.GetComponent<MeshCollider>();

            //Get the parent (world)
            m_parent = this.GetComponentInParent<WorldGenerator>();

            //Generate the current chunk
            mainpos = transform.position;
            t = new Thread(new ThreadStart(GenerateHeightMap));
            t.Start();
            //GenerateHeightMap();
            //UpdateMesh();
        }
        void Update()
        {
            if(State == ChunkState.NeedMeshUpdate)
            {
                UpdateMesh();
            }
        }
        #endregion
        //*************************************************************
        /// <summary>
        /// Generate the chunk mesh
        /// </summary>
        void GenerateHeightMap()
        {
            //Update the chunk state
            State = ChunkState.CreatingHeightMap;

            for (int x = 0; x < XSize; x++)
            {
                for (int y = 0; y < YSize; y++)
                {
                    for (int z = 0; z < ZSize; z++)
                    {
                        //Vector3 currBlockPos = new Vector3(
                        //    x + this.transform.position.x,
                        //    y + this.transform.position.y,
                        //    z + this.transform.position.z);  
                        Vector3 currBlockPos = new Vector3(
                            x + mainpos.x,
                            y + mainpos.y,
                            z + mainpos.z);

                        Blocks[x, y, z] = m_parent.GetWorldBlock(currBlockPos);

                        //Guard: blocks to ignore
                        if (Blocks[x, y, z] == BlockType.NULL)
                            continue;

                        //Set the visible faces
                        if (m_parent.GetWorldBlock(currBlockPos.x + 1, currBlockPos.y, currBlockPos.z) == BlockType.NULL)
                        {
                            Block.EastFace(x, y, z, m_vertices, m_triangles, m_faceCount);
                            setFaceTexture(x, y, z, FaceType.East);
                            m_faceCount++;
                        }
                        if (m_parent.GetWorldBlock(currBlockPos.x, currBlockPos.y + 1, currBlockPos.z) == BlockType.NULL)
                        {
                            Block.TopFace(x, y, z, m_vertices, m_triangles, m_faceCount);
                            setFaceTexture(x, y, z, FaceType.Top);
                            m_faceCount++;
                        }
                        if (m_parent.GetWorldBlock(currBlockPos.x, currBlockPos.y, currBlockPos.z + 1) == BlockType.NULL)
                        {
                            Block.NorthFace(x, y, z, m_vertices, m_triangles, m_faceCount);
                            setFaceTexture(x, y, z, FaceType.North);
                            m_faceCount++;
                        }
                        if (m_parent.GetWorldBlock(currBlockPos.x - 1, currBlockPos.y, currBlockPos.z) == BlockType.NULL)
                        {
                            Block.WestFace(x, y, z, m_vertices, m_triangles, m_faceCount);
                            setFaceTexture(x, y, z, FaceType.West);
                            m_faceCount++;
                        }
                        if (m_parent.GetWorldBlock(currBlockPos.x, currBlockPos.y - 1, currBlockPos.z) == BlockType.NULL)
                        {
                            Block.BottomFace(x, y, z, m_vertices, m_triangles, m_faceCount);
                            setFaceTexture(x, y, z, FaceType.Bottom);
                            m_faceCount++;
                        }
                        if (m_parent.GetWorldBlock(currBlockPos.x, currBlockPos.y, currBlockPos.z - 1) == BlockType.NULL)
                        {
                            Block.SouthFace(x, y, z, m_vertices, m_triangles, m_faceCount);
                            setFaceTexture(x, y, z, FaceType.South);
                            m_faceCount++;
                        }
                    }
                }
            }

            //Update the chunk state
            State = ChunkState.NeedMeshUpdate;
        }
        /// <summary>
        /// Update the mesh parameters, vertices, triangles and uv
        /// </summary>
        void UpdateMesh()
        {
            //Reset mesh
            m_mesh.Clear();
            m_mesh.vertices = m_vertices.ToArray();
            m_mesh.uv = m_uv.ToArray();
            m_mesh.triangles = m_triangles.ToArray();
            m_mesh.RecalculateNormals();
            m_mesh.Optimize();

            //Set the textures
            switch (m_parent.TextureMode)
            {
                case TextureMode.SOLID_COLOR:
                    m_mesh.colors = m_colors.ToArray();
                    break;
                case TextureMode.CANVAS_TEXTURE:
                    break;
                default:
                    break;
            }

            //Setup the collider
            m_collider.sharedMesh = m_mesh;

            //Clear the memory
            m_vertices.Clear();
            m_uv.Clear();
            m_triangles.Clear();
            m_colors.Clear();
            m_faceCount = 0;

            State = ChunkState.Idle;
        }
        //*************************************************************
        private void setFaceTexture(int x, int y, int z, FaceType face)
        {
            switch (m_parent.TextureMode)
            {
                case TextureMode.SOLID_COLOR:
                    for (int i = 0; i < 4; i++)
                    {
                        //m_colors.Add(Block.GetColor(Blocks[x, y, z]));
                    }
                    break;
                case TextureMode.CANVAS_TEXTURE:
                    m_uv.AddRange(Block.GetTexture(Blocks[x, y, z]));
                    break;
                default:
                    break;
            }
        }
    }
}
