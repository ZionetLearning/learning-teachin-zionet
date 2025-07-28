import { useStyles } from "./style";

interface SpeakerProps {
    mode?: "normal" | "slow";
    onClick: () => void;
}

export const Speaker = ({ mode = "normal", onClick }: SpeakerProps) => {
    const classes = useStyles();
    const emoji = mode === "normal" ? "🔊" : "🐢";
    const label = mode === "normal" ? "Play audio" : "Play audio slowly (0.6x)";

    return (
        <button
            type="button"
            className={classes.speaker}
            onClick={onClick}
            aria-label={label}
            title={label}
            data-mode={mode}
        >
            <span style={mode === "slow" ? { transform: "translateY(-5px)" } : undefined}
                aria-hidden="true">{emoji}</span>
        </button>
    );
};
