namespace FarseerPhysics.Dynamics.Joints
{
	public sealed class JointEdge
	{
		public FarseerJoint Joint;

		public JointEdge Next;

		public Body Other;

		public JointEdge Prev;
	}
}
