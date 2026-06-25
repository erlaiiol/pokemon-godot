using Godot;
using System;

namespace pokemonGodot.Scripts.Core
{

	public partial class Globals : Node
	{
		public static Globals Instance
		{
			get;
			private set;
		}

		[ExportCategory("Gameplay")] 
		[Export] public int GRID_SIZE = 16;
		[Export] ulong Seed = 1337;

		private RandomNumberGenerator RandomNumberGenerator;
		

		public override void _Ready()
		{
			Instance = this;

			RandomNumberGenerator = new()
			{
				Seed = Seed
			};

			Logger.Debug("Loading Globals...");
			Logger.Info("Loading Globals...");
			Logger.Warning("Loading Globals...");
			Logger.Error("Loading Globals...");
		}

		public static RandomNumberGenerator GetRandomNumberGenerator()
		{
			return Instance.RandomNumberGenerator;
		}

		public static Vector2 SnapToGrid(Vector2 worldPos)
		{
			float half = Instance.GRID_SIZE / 2f;
			return new Vector2(
				Mathf.Round((worldPos.X - half) / Instance.GRID_SIZE) * Instance.GRID_SIZE + half,
				Mathf.Round((worldPos.Y - half) / Instance.GRID_SIZE) * Instance.GRID_SIZE + half
			);
		}
	}

}
