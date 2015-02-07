using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

namespace Amps
{
	[System.Serializable]
	public class MultiFunctionStack : BaseStack
	{

#if UNITY_EDITOR
		// INITIALIZE //
		//
		override public void Initialize(AmpsBlueprint theOwnerBlueprint, AmpsHelpers.eStackFunction sf, string mt)
		{
			base.Initialize(theOwnerBlueprint, sf, mt);

			isParticleStack = false;
			isVector3Stack = false;
		}
#endif

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();
		}

		// EVALUATE SPAWN RATE//
		//
		public void Evaluate_SpawnRate()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifySpawnRate) mfm.Evaluate_SpawnRate();
				}
			}
		}

		// EVALUATE DEATH CONDITION//
		//
		public void Evaluate_DeathCondition()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyDeathCondition) mfm.Evaluate_DeathCondition();
				}
			}
		}

		// EVALUATE DEATH DURATION//
		//
		public void Evaluate_DeathDuration()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyDeathDuration) mfm.Evaluate_DeathDuration();
				}
			}
		}

		// EVALUATE CUSTOM SCALAR//
		//
		public void Evaluate_CustomScalar()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyCustomScalar) mfm.Evaluate_CustomScalar();
				}
			}
		}

		// EVALUATE CUSTOM VECTOR//
		//
		public void Evaluate_CustomVector()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyCustomVector) mfm.Evaluate_CustomVector();
				}
			}
		}

		// EVALUATE ACCELERATION//
		//
		public void Evaluate_Acceleration()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyAcceleration) mfm.Evaluate_Acceleration();
				}
			}
		}

		// EVALUATE VELOCITY//
		//
		public void Evaluate_Velocity()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyVelocity) mfm.Evaluate_Velocity();
				}
			}
		}

		// EVALUATE POSITION//
		//
		public void Evaluate_Position()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyPosition) mfm.Evaluate_Position();
				}
			}
		}

		// EVALUATE ROTATION RATE//
		//
		public void Evaluate_RotationRate()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyRotationRate) mfm.Evaluate_RotationRate();
				}
			}
		}

		// EVALUATE ROTATION //
		//
		public void Evaluate_Rotation()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyRotation) mfm.Evaluate_Rotation();
				}
			}
		}

		// EVALUATE SCALE//
		//
		public void Evaluate_Scale()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyScale) mfm.Evaluate_Scale();
				}
			}
		}

		// EVALUATE COLOR//
		//
		public void Evaluate_Color()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyColor) mfm.Evaluate_Color();
				}
			}
		}

		// EVALUATE PIVOT OFFSET//
		//
		public void Evaluate_PivotOffset()
		{
			foreach (BaseModule m in modules)
			{
				if (m.isEnabled)
				{
					BaseMultiFunctionModule mfm = m as BaseMultiFunctionModule;
					if (mfm.modifyPivotOffset) mfm.Evaluate_PivotOffset();
				}
			}
		}

		// EVALUATE //
		//
		override public void Evaluate()
		{
			// Shouldn't be called, only the specialized ones above.
		}
	}
}