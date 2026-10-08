namespace GoddessTempleDiscovery.Game.Credits;

/// <summary>
/// Jeremy's Note on Cultural Heritage and History (DESIGN decision 3), verbatim: printed on the Credits screen, in the
/// Field Journal and in the README. Never paraphrase it; a revision of the note replaces this text whole.
/// </summary>
public static class HeritageNote
{
    /// <summary>The note's title.</summary>
    public const string Title = "Note on Cultural Heritage and History";

    /// <summary>The first paragraph.</summary>
    public const string FirstParagraph =
        "In exploring the sacred history of ancient Mesopotamia and the important cultural heritage of the area — that, I believe, provided the foundation of cultural heritage for my own European ancestors — I wish to acknowledge the deep and ongoing pain felt by the people of Iraq (and neighboring countries) regarding the extraction of their national heritage. The early archeological excavations of sites like Uruk, Ur and Babylon occurred during a period of immense power imbalance and colonial influence. While this document references the people involved and findings of a portion of those missions, it is not an endorsement of the \"heritage-looting\", colonization, or cultural extraction that resulted in so many significant artifacts being removed from their homeland. I hope to not be another agent of cultural appropriation; but instead to honor the legacy of the people who have lived in the Tigris and Euphrates valleys for millennia; and I present this work with the utmost respect for their enduring connection to this history. My intent is to celebrate the numinous presence of the Divine as understood through ancient and explored through modern sources, without diminishing the modern right of Iraqis to their own cultural and historical birthright.";

    /// <summary>The second paragraph.</summary>
    public const string SecondParagraph =
        "Also, this creative work references German archaeologists and scholars — some with documented Nazi-party ties, some who participated in Axis-aligned political activity in Iraq (notably around the 1941 Rashid Ali coup and its aftermath). No part of this creative work is intended to celebrate individuals who espoused or were aligned with Nazi ideology, or who were involved in actions against the Iraqi people.";

    /// <summary>Both paragraphs, separated by a blank line.</summary>
    public const string Text = FirstParagraph + "\n\n" + SecondParagraph;
}
