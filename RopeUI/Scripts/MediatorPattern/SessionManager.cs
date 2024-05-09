using Godot;
using Rope.Abstractions.Models;
using Rope.Abstractions.Reflection;
using RopeCSharp;
using RopeUI.Scripts.Dialogues;
using System;
using System.IO;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text.Json;

namespace RopeUI.Scripts.MediatorPattern;
public partial class SessionManager : Node, IPlugin
{
    [Export]
    public PackedScene? LoadScriptPopupPack;
    [Export]
    public PackedScene? NewScriptPopupPack;

    private DependencyManger? _dependencyManger;

    public DataBase Database { get; private set; } = Service.LoadAssembly(Assembly.GetAssembly(AssemblyConfigs.AssemblyEntry)!);

    public BehaviorSubject<RopeScript?> ScriptAccouncement { get; private set; } = new(null);
    public BehaviorSubject<RopeAction?> ActionSelectio { get; private set; } = new(null);

    public void ConfigureServices(DependencyManger depdencyManager)
    {
        if (Database == null)
        {
            GD.PushError("Session manager failed to load reflection database");
            QueueFree();
            return;
        }

        if (!depdencyManager.AddSingleton(this))
        {
            QueueFree();
        }
    }

    public void ContainerSetup(DependencyManger _) { }

    public void InitiateLoadScript()
    {
        if (Database == null || ScriptAccouncement.Value != null)
            return;

        OpenFileDialog scriptDialogue = (OpenFileDialog)LoadScriptPopupPack!.Instantiate();
        scriptDialogue.ProxiedFileSelection += OnConfirmLoadScript;
        AddChild(scriptDialogue);
    }

    private void OnConfirmLoadScript(Node sender, string path)
    {
        try
        {
            // load script
            using FileStream scriptFile = File.Open(path, FileMode.Open, System.IO.FileAccess.Read, FileShare.Read);
            RopeScript? newScript = JsonSerializer.Deserialize<RopeScript>(scriptFile);

            // safety check
            if (newScript == null || !SanityCheckScript(newScript))
                throw new Exception();

            // broadcast
            ScriptAccouncement.OnNext(newScript);
            
            // close popup
            sender.QueueFree();
        }
        catch (Exception e)
        {
            GD.PrintErr($"unable to load script from {path}: {e}");
        }
    }

    private bool SanityCheckScript(RopeScript script)
    {
        // check context
        if (!Database!.ContextTypes.TryGetValue(script.Context, out ContextType? contextData))
            return false;

        // check actions
        foreach (RopeNode node in script.Nodes)
        {
            foreach (RopeAction action in node.Actions)
            {
                if (!contextData.Actions.ContainsKey(action.Action))
                    return false;
            }
        }

        return true;
    }

    public void TryCreateNewScript()
    {
        var popup = (NewScriptPopup)NewScriptPopupPack!.Instantiate();
        AddChild(popup);
        popup.DisplayOptions(Database!);
        popup.NewScriptConfirmed += OnConfirmNewScript;
    }

    private void OnConfirmNewScript(Node sender, string newName, string newNamespace, string type)
    {
        RopeScript script = new()
        {
            Name = newName,
            Namespace = newNamespace,
            Context = type,
            Nodes = []
        };

        ScriptAccouncement.OnNext(script);
        sender.QueueFree();
    }

    public void TryCloseScript()
    {
        ScriptAccouncement.OnNext(null);
    }

    public string SerializeCode()
    {
        if (ScriptAccouncement.Value == null || Database == null)
            return string.Empty;

        string output = Service.SerializeCode(ScriptAccouncement.Value, Database.ContextTypes);
        return output;
    }

    public string SerializeJson()
    {
        if (ScriptAccouncement.Value == null)
            return string.Empty;

        string output = JsonSerializer.Serialize(ScriptAccouncement.Value);
        return output;
    }
}
