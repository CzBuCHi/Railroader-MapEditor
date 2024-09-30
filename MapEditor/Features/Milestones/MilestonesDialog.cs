using System;
using System.Collections.Generic;
using System.Linq;
using Game.Events;
using Game.Progression;
using MapEditor.Features.Abstract;
using Railloader;
using UI.Builder;
using UI.Common;

namespace MapEditor.Features.Milestones;

public sealed class MilestonesDialog(IUIHelper uiHelper) : DialogBase(uiHelper)
{
    #region Manage

    private static MilestonesDialog? _Instance;

    public static void Show(IUIHelper uiHelper) {
        Show(ref _Instance, () => new MilestonesDialog(uiHelper));
    }

    public static void Close() {
        Close(ref _Instance);
    }

    #endregion

    protected override int             WindowWidth    => 800;
    protected override int             WindowHeight   => 600;
    protected override Window.Position WindowPosition => Window.Position.UpperRight;
    protected override string          WindowTitle    => "Map Editor | Milestone manager";

    private readonly UIState<string?> _SelectedItem = new(null);

    protected override void BuildWindow(UIPanelBuilder builder) {
        var shared = Progression.Shared;
        if (shared == null) {
            builder.AddLabel("Milestones not available. Please quit and reload this save.");
            return;
        }

        builder.RebuildOnEvent<ProgressionStateDidChange>();

        var prerequisites = shared.Sections!
                                  .SelectMany(o => o.prerequisiteSections, (o, p) => (o, p))
                                  .GroupBy(t => t.p, t => t.o)
                                  .ToDictionary(o => o.Key, o => o.ToArray());

        var sections = shared.Sections!.OrderBy(SectionIndexForSection)
                             .Select(section => {
                                 var text = section.Unlocked || section.Available ? section.displayName! : "<i>" + section.displayName + "</i>";
                                 return new UIPanelBuilder.ListItem<Section>(section.identifier!, section, SectionNameForSection(section), text);
                             }).ToList();

        builder.AddListDetail(sections, _SelectedItem, BuildDetail(prerequisites));
    }

    private static Action<UIPanelBuilder, Section?> BuildDetail(Dictionary<Section, Section[]> prerequisites) => (builder, section) => BuildDetail(builder, section, prerequisites);

    private static void BuildDetail(UIPanelBuilder builder, Section? section, Dictionary<Section, Section[]> prerequisites) {
        if (section == null) {
            builder.AddLabel("No milestone selected.");
            builder.AddExpandingVerticalSpacer();
            return;
        }

        if (section.Available) {
            var length = section.deliveryPhases!.Length;
            builder.AddTitle(section.displayName!, $"{section.FulfilledCount}/{length} Phases Complete");
            builder.AddLabel(section.description!);
            builder.AddButton("Advance", () => Advance(section));
            if (length > 1) {
                builder.AddButton("Advance one phase", () => Progression.Shared!.Advance(section));
            }
        } else if (section.Unlocked) {
            builder.AddTitle(section.displayName!, "Completed!");
            builder.AddLabel(section.description!);
            builder.AddButton("Revert", () => Revert(section, prerequisites));
        } else {
            builder.AddTitle(section.displayName!, "Not yet available.");
            builder.AddLabel(section.description!);
            builder.AddSection("Prerequisites", prerequisite => {
                foreach (var prerequisiteSection in section.prerequisiteSections!) {
                    prerequisite.AddLabel(prerequisiteSection.displayName + " - " + (prerequisiteSection.Unlocked ? "Completed" : "Not Complete"));
                }
            });
            builder.AddButton("Advance Prerequisites", () => AdvancePrerequisites(section));
        }

        builder.AddExpandingVerticalSpacer();
    }

    private static void AdvancePrerequisites(Section section) {
        foreach (var prerequisite in section.prerequisiteSections!.Where(o => !o.Unlocked)) {
            AdvancePrerequisites(prerequisite);
            Advance(prerequisite);
        }
    }

    private static void Advance(Section section) {
        foreach (var _ in section.deliveryPhases!) {
            Progression.Shared!.Advance(section);
        }
    }

    private static void Revert(Section section, Dictionary<Section, Section[]> prerequisites) {
        if (prerequisites.TryGetValue(section, out var sectionPrerequisites) && sectionPrerequisites != null) {
            foreach (var prerequisite in sectionPrerequisites.Where(o => o.Unlocked)) {
                Revert(prerequisite, prerequisites);
            }
        }

        global::UI.Console.Console.shared.AddLine($"Revert: {section.displayName}");
        foreach (var _ in section.deliveryPhases!) {
            Progression.Shared!.Revert(section);
        }
    }

    private static int SectionIndexForSection(Section section) =>
        section.Unlocked       ? 3 :
        !section.Available     ? 2 :
        section.PaidCount <= 0 ? 1 : 0;

    private static string SectionNameForSection(Section section) =>
        section.Unlocked       ? "Complete" :
        !section.Available     ? "Not Yet Available" :
        section.PaidCount <= 0 ? "Available" :
                                 "In Progress";
}
