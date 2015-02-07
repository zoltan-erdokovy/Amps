using UnityEngine;
using System;
using System.Collections.Generic;

namespace Amps
{
	[System.Serializable]
	public class PropertyReference : ScriptableObject
	{
		public BaseModule module;
		public BaseProperty property;

		public void Initialize(BaseModule m, BaseProperty p)
		{
			module = m;
			property = p;
			name = "PropertyReference";
		}
	}
}