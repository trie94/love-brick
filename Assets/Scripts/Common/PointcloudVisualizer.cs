namespace Love.Common
{
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Visualize the point cloud.
    /// </summary>
    public class PointcloudVisualizer : MonoBehaviour
    {
        private const int k_MaxPointCount = 61440;

        private Mesh m_Mesh;

        private Vector3[] m_Points = new Vector3[k_MaxPointCount];

        /// <summary>
        /// Unity start.
        /// </summary>
        public void Start()
        {
            m_Mesh = GetComponent<MeshFilter>().mesh;
            m_Mesh.Clear();
        }

        /// <summary>
        /// Unity update.
        /// </summary>
        public void Update()
        {
            // Fill in the data to draw the point cloud.
            if (Frame.PointCloud.IsUpdatedThisFrame)
            {
                // Copy the point cloud points for mesh verticies.
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    m_Points[i] = Frame.PointCloud.GetPoint(i);
                }

                // Update the mesh indicies array.
                int[] indices = new int[Frame.PointCloud.PointCount];
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    indices[i] = i;
                }

                m_Mesh.Clear();
                m_Mesh.vertices = m_Points;
                m_Mesh.SetIndices(indices, MeshTopology.Points, 0);
            }
        }
    }
}