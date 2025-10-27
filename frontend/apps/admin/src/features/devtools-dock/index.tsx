import { createPortal } from "react-dom";
import { Box } from "@mui/material";
import { useSignalR, useDevTools } from "@admin/hooks";
import { SignalRPanel } from "./components";
import { useStyles } from "./style";

export const DevToolsDock = ({
  label = "DevTools",
  shortcut = "Ctrl+`",
}: {
  label?: string;
  shortcut?: string;
}) => {
  const classes = useStyles();
  const { isOpen, setOpen, isHebrew } = useDevTools();
  const { status: signalRStatus } = useSignalR();

  const portalRoot = typeof document !== "undefined" ? document.body : null;
  if (!portalRoot) return null;

  const healthClass =
    signalRStatus === "connected"
      ? "ok"
      : signalRStatus === "reconnecting"
        ? "warn"
        : "";

  return createPortal(
    <>
      <button
        type="button"
        className={classes.floater}
        style={isHebrew ? { left: 16 } : { right: 16 }}
        onClick={() => setOpen(!isOpen)}
        aria-label="Toggle DevTools"
        title={`${label} (${shortcut})`}
      >
        <span className={`${classes.dot} ${healthClass}`} />
        <span className={classes.label}>{label}</span>
      </button>

      {isOpen && (
        <Box className={classes.dock}>
          <Box className={classes.header}>
            <strong style={{ fontSize: 12 }}>{label}</strong>
            <Box className={classes.shortcutWrapper}>{shortcut}</Box>
          </Box>
          <Box className={classes.body}>
            <SignalRPanel />
          </Box>
        </Box>
      )}
    </>,
    portalRoot,
  );
};
