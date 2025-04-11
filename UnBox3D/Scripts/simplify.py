import sys
import trimesh
import open3d as o3d
import pyvista as pv

def simplify_with_qec(input_path, output_path, simplification_factor):

    mesh = trimesh.load(input_path, force='mesh')
    vertices = mesh.vertices
    faces = mesh.faces
    o3d_mesh = o3d.geometry.TriangleMesh()
    o3d_mesh.vertices = o3d.utility.Vector3dVector(vertices)
    o3d_mesh.triangles = o3d.utility.Vector3iVector(faces)
    simplified_open3d = o3d_mesh.simplify_quadric_decimation(int(len(o3d_mesh.triangles) * (1 - simplification_factor)))
    simplified_open3d.compute_vertex_normals()
    o3d.io.write_triangle_mesh(output_path, simplified_open3d)

def simplify_with_fqd(input_path, output_path, simplification_factor):
    pv_mesh = pv.read(input_path)
    simplified = pv_mesh.decimate(simplification_factor)
    simplified = simplified.compute_normals()
    simplified.save(output_path)

def simplify_with_vc(input_path, output_path, simplification_factor):
    mesh = trimesh.load_mesh(input_path)
    vertices = mesh.vertices
    faces = mesh.faces
    o3d_mesh = o3d.geometry.TriangleMesh()
    o3d_mesh.vertices = o3d.utility.Vector3dVector(vertices)
    o3d_mesh.triangles = o3d.utility.Vector3iVector(faces)
    bounds = mesh.bounds[1] - mesh.bounds[0]
    voxel_size = min(bounds) * simplification_factor * 0.1
    voxel_mesh = o3d_mesh.simplify_vertex_clustering(voxel_size=voxel_size)
    voxel_mesh.compute_vertex_normals()
    o3d.io.write_triangle_mesh(output_path, voxel_mesh)

if __name__ == "__main__":
    input_path = sys.argv[1]
    output_path = sys.argv[2]
    method = sys.argv[3].lower()
    simplification_factor = float(sys.argv[4])

    if method == "quadric_edge_collapse":
        simplify_with_qec(input_path, output_path, simplification_factor)
    elif method == "fast_quadric_decimation":
        simplify_with_fqd(input_path, output_path, simplification_factor)
    elif method == "vertex_clustering":
        simplify_with_vc(input_path, output_path, simplification_factor)
    else:
        print(f"Unknown method: {method}")
        sys.exit(1)