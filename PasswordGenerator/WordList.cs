namespace PasswordGenerator
{
    /// <summary>
    ///     A small built-in list of common, readable words used by <see cref="PassphraseGenerator" />.
    ///     This is intentionally compact (not a full diceware list); each word contributes
    ///     log2(<see cref="Words" />.Length) bits of entropy.
    /// </summary>
    internal static class WordList
    {
        public static readonly string[] Words =
        {
            "able", "acid", "acorn", "actor", "agile", "alarm", "album", "alert", "alley", "amber",
            "amend", "angle", "ankle", "apple", "april", "arena", "armor", "arrow", "aside", "asset",
            "atlas", "audio", "aunt", "avoid", "awake", "award", "bacon", "badge", "baker", "banjo",
            "barge", "basil", "basin", "beach", "beard", "beast", "begin", "berry", "birch", "bison",
            "blade", "blaze", "blend", "blink", "block", "bloom", "board", "boost", "booth", "brace",
            "brain", "brand", "brave", "bread", "brick", "brief", "broad", "brook", "brush", "buddy",
            "bunch", "cabin", "cable", "cacao", "camel", "candy", "canoe", "cargo", "carol", "carve",
            "catch", "cedar", "chalk", "charm", "chase", "cheek", "chess", "chief", "chili", "chime",
            "civic", "claim", "clamp", "clay", "clean", "clear", "cliff", "climb", "clock", "cloud",
            "clove", "coast", "cobra", "cocoa", "comet", "coral", "couch", "cover", "crane", "crate",
            "crisp", "crown", "crumb", "curve", "daisy", "dance", "delta", "diner", "ditch", "diver",
            "dodge", "donor", "draft", "drama", "dream", "dress", "drift", "drink", "drive", "eagle",
            "early", "earth", "ember", "emery", "enjoy", "equal", "ethos", "every", "fable", "fancy",
            "feast", "fiber", "field", "final", "flame", "flank", "flash", "fleet", "flint", "float",
            "flock", "flora", "flute", "focus", "forge", "frame", "frost", "fruit", "gauge", "ghost",
            "giant", "glade", "glass", "glide", "globe", "glory", "grace", "grain", "grand", "grape",
            "grasp", "grass", "green", "grove", "guide", "habit", "happy", "harbor", "haven", "hazel",
            "heart", "hedge", "honey", "horse", "hotel", "house", "human", "humor", "ideal", "image",
            "index", "inlet", "ivory", "jelly", "jewel", "jolly", "joust", "judge", "juice", "karma",
            "kayak", "ketch", "kitten", "knack", "knife", "koala", "label", "lance", "laser", "latch",
            "layer", "leaf", "ledge", "lemon", "lever", "light", "lilac", "linen", "llama", "lodge",
            "lotus", "lunar", "lyric", "magic", "maize", "mango", "maple", "march", "marsh", "match",
            "medal", "melon", "mercy", "metro", "manor", "mocha", "model", "money", "month", "motor",
            "mound", "mount", "mouse", "music", "noble", "north", "novel", "ocean", "olive", "onion",
            "opera", "orbit", "otter", "owl", "paint", "panda", "paper", "party", "pasta", "patch",
            "peach", "pearl", "pedal", "perch", "piano", "pilot", "pixel", "pizza", "plank", "plant",
            "plaza", "plume", "polar", "porch", "prism", "prize", "proud", "pulse", "punch", "quail",
            "quartz", "queen", "quest", "quiet", "quill", "quilt", "radar", "rapid", "raven", "reach",
            "realm", "rebel", "relay", "rhino", "ridge", "river", "roast", "robin", "rocky", "rover",
            "royal", "ruby", "salad", "salsa", "sandy", "scarf", "scout", "shade", "shard", "shark",
            "sheep", "shelf", "shell", "shine", "shore", "siren", "skate", "skiff", "slate", "sleek",
            "slope", "smile", "smoke", "snail", "solar", "sonic", "spade", "spark", "spice", "spine",
            "spire", "spoon", "sport", "spray", "spruce", "stack", "stage", "stalk", "stamp", "stark",
            "steam", "steel", "stork", "storm", "story", "stove", "straw", "strip", "sugar", "swift",
            "table", "tango", "teal", "thorn", "tidal", "tiger", "toast", "topaz", "torch", "totem",
            "tower", "trail", "treat", "trend", "tribe", "trout", "tulip", "tundra", "ultra", "umber",
            "unity", "urban", "valley", "value", "vapor", "vault", "venus", "vigor", "villa", "vinyl",
            "viola", "vivid", "vocal", "wafer", "wagon", "waltz", "water", "wheat", "whale", "wharf",
            "wheel", "whisk", "willow", "windy", "woven", "yacht", "yearn", "yield", "zebra", "zesty"
        };
    }
}
