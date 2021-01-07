using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class Root : Control
{
	private Label StatusLabel;
	private OptionButton Groups;
	private VBoxContainer Entries;

	private PackedScene ButtonScene;

	private Dictionary<string, List<string>> EntryData;

	UdpClient UdpClient;
	byte[] RequestData;
	IPEndPoint GlobalIP;
	private bool Connected;
	TcpClient TcpClient;

	public override void _Ready()
	{
		StatusLabel = GetNode<Label>("Main/Status");
		StatusLabel.Text = "Connecting.";
		Groups = GetNode<OptionButton>("Main/Groups");
		Entries = GetNode<VBoxContainer>("Main/Scroll/Entries");

		Groups.Connect("item_selected", this, nameof(OnGroupSelected));

		ButtonScene = GD.Load<PackedScene>("Entry.tscn");

		UdpClient = new UdpClient();
		RequestData = Encoding.ASCII.GetBytes("UnarySoundboardClient");
		GlobalIP = new IPEndPoint(IPAddress.Any, 0);

		UdpClient.EnableBroadcast = true;
		UdpClient.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888));

		var ServerResponseData = UdpClient.Receive(ref GlobalIP);
		var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);

		UdpClient.Close();

		if (ServerResponse == "UnarySoundboardServer")
		{
			TcpClient = new TcpClient();

			try
			{
				TcpClient.Connect(GlobalIP.Address, 9999);

				if (TcpClient.Connected)
				{
					StatusLabel.Text = "Established connection.";
				}
				else
				{
					StatusLabel.Text = "Failed to connect.";
				}
			}
			catch (Exception)
			{
				StatusLabel.Text = "Got exception while connecting.";
			}
		}

	}

	public void OnGroupSelected(int id)
	{
		GD.Print("SELECTED!");

		for (int i = 0; i < Entries.GetChildCount(); ++i)
		{
			Entries.GetChild(i).QueueFree();
		}

		GD.Print("Group: " + Groups.GetItemText(id));

		foreach (var Entry in EntryData[Groups.GetItemText(id)])
		{
			EntryButton NewEntryButton = (EntryButton)ButtonScene.Instance();
			NewEntryButton.Text = Entry;
			Entries.AddChild(NewEntryButton);
			GD.Print("Added child " + Entry);
		}
	}

	public void OnSend(string EntryPath)
	{
		byte[] Result = Encoding.UTF8.GetBytes(Groups.GetItemText(Groups.GetSelectedId()) + '\\' + EntryPath);
		TcpClient.GetStream().Write(Result, 0, Result.Length);
	}

	public override void _Process(float delta)
	{
		if (TcpClient.Connected)
		{
			if (TcpClient.Available != 0)
			{
				byte[] Bytes = new byte[TcpClient.Available];
				TcpClient.GetStream().Read(Bytes, 0, TcpClient.Available);
				EntryData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(Encoding.UTF8.GetString(Bytes));

				foreach (var Category in EntryData)
				{
					GD.Print("Category: " + Category.Key);

					foreach (var Entry in Category.Value)
					{
						GD.Print("Entry: " + Entry);
					}

				}

				Groups.AddSeparator();

				foreach (var Category in EntryData)
				{

					Groups.AddItem(Category.Key);
				}

				Groups.AddSeparator();


			}
		}
	}

	public override void _ExitTree()
	{
		if (TcpClient.Connected)
		{
			TcpClient.Close();
		}
	}
}
