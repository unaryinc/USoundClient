using Godot;
using System;

public class EntryButton : Button
{
	private Timer Timer;

	public override void _Ready()
	{
		Timer = new Timer();
		Connect("pressed", this, nameof(OnPressed));
		Timer.Connect("timeout", this, nameof(OnTimeout));
		AddChild(Timer);
	}

	public void OnPressed()
	{
		Timer.Start(1.0f);
		Modulate = Color.Color8(0, 128, 0);
		GetNode<Root>("/root/Control").OnSend(Text);
	}

	public void OnTimeout()
	{
		Timer.Stop();
		Modulate = Color.Color8(255, 255, 255);
	}

}
