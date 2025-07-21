import { createUseStyles } from "react-jss";

export const useAvatarOuStyles = createUseStyles({
  container: {
    height: "100vh",
    background:
      "linear-gradient(135deg, #f8fafc 0%, #e2e8f0 50%, #cbd5e1 100%)",
    padding: "2rem",
    overflow: "hidden",
  },

  wrapper: {
    width: "100%",
    height: "100%",
    maxWidth: "100%",
    minWidth: "1100px",
    margin: "0 auto",
    display: "flex",
    flexDirection: "column",
  },

  header: {
    textAlign: "center",
    marginBottom: "1rem",
    flexShrink: 0,
  },

  title: {
    fontSize: "1.75rem",
    fontWeight: "600",
    color: "#1e293b",
    marginBottom: "0.5rem",
  },

  subtitle: {
    fontSize: "1rem",
    color: "#64748b",
    fontWeight: "500",
  },

  headerDivider: {
    width: "4rem",
    height: "0.25rem",
    background: "#3b82f6",
    margin: "0.75rem auto 0",
    borderRadius: "2px",
  },

  mainCard: {
    background: "white",
    borderRadius: "1rem",
    boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
    padding: "2.5rem",
    border: "1px solid #e2e8f0",
    flex: 1,
    display: "flex",
    gap: "3rem",
    overflow: "hidden",
  },

  avatarSection: {
    background: "#f8fafc",
    borderRadius: "0.75rem",
    padding: "2rem",
    border: "1px solid #e2e8f0",
    // flex: '0 0 450px',
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
  },

  avatarContainer: {
    display: "flex",
    justifyContent: "center",
    marginBottom: "1.5rem",
  },

  avatarWrapper: {
    position: "relative",
  },

  avatarGlow: {
    position: "absolute",
    inset: "-0.5rem",
    background: "#3b82f6",
    borderRadius: "50%",
    filter: "blur(0.5rem)",
    opacity: 0.2,
    animation: "$pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite",
  },

  avatarFrame: {
    position: "relative",
    background: "white",
    borderRadius: "50%",
    padding: "1.5rem",
    boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
    border: "2px solid #e2e8f0",
  },

  statusContainer: {
    textAlign: "center",
  },

  statusBadge: {
    display: "inline-flex",
    alignItems: "center",
    gap: "0.5rem",
    padding: "0.75rem 1.5rem",
    borderRadius: "0.5rem",
    fontSize: "1rem",
    fontWeight: "500",
    boxShadow: "0 1px 3px 0 rgb(0 0 0 / 0.1)",
    border: "1px solid",
    transition: "all 0.3s ease",
  },

  statusBadgePlaying: {
    background: "#10b981",
    color: "white",
    borderColor: "#10b981",
  },

  statusBadgeIdle: {
    background: "#64748b",
    color: "white",
    borderColor: "#64748b",
  },

  statusDot: {
    width: "0.5rem",
    height: "0.5rem",
    borderRadius: "50%",
    background: "white",
  },

  statusDotPlaying: {
    animation: "$pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite",
  },

  controlsSection: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
    gap: "1.5rem",
    overflow: "auto",
  },

  inputLabel: {
    display: "block",
    fontSize: "1.125rem",
    fontWeight: "600",
    color: "#374151",
    marginBottom: "0.75rem",
    "& > span": {
      display: "inline-flex",
      alignItems: "center",
      gap: "0.5rem",
    },
  },

  textareaWrapper: {
    position: "relative",
  },

  textarea: {
    width: "100%",
    padding: "1rem",
    border: "1px solid #d1d5db",
    borderRadius: "0.5rem",
    fontSize: "1rem",
    textAlign: "right",
    background: "white",
    color: "#1f2937",
    boxShadow: "0 1px 3px 0 rgb(0 0 0 / 0.1)",
    fontWeight: "400",
    outline: "none",
    transition: "border-color 0.3s ease",
    resize: "vertical",

    "&:focus": {
      borderColor: "#3b82f6",
      boxShadow: "0 0 0 3px rgb(59 130 246 / 0.1)",
    },

    "&::placeholder": {
      color: "#9ca3af",
    },
  },

  charCounter: {
    position: "absolute",
    bottom: "0.75rem",
    left: "0.75rem",
    background: "#64748b",
    color: "white",
    padding: "0.25rem 0.5rem",
    borderRadius: "0.25rem",
    fontSize: "0.75rem",
    fontWeight: "500",
  },

  samplesGrid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))",
    gap: "0.5rem",
  },

  sampleButton: {
    padding: "0.75rem 1rem",
    color: "#374151",
    borderRadius: "0.5rem",
    fontSize: "0.875rem",
    fontWeight: "500",
    boxShadow: "0 1px 3px 0 rgb(0 0 0 / 0.1)",
    border: "1px solid #d1d5db",
    cursor: "pointer",
    transition: "all 0.3s ease",
    background: "white",

    "&:hover": {
      background: "#f8fafc",
      borderColor: "#3b82f6",
      color: "#3b82f6",
    },
  },

  sampleButton0: {
    background: "white",
  },

  sampleButton1: {
    background: "white",
  },

  sampleButton2: {
    background: "white",
  },

  sampleButton3: {
    background: "white",
  },

  sampleButton4: {
    background: "white",
  },

  buttonsContainer: {
    display: "flex",
    flexDirection: "column",
    gap: "1.5rem",
    paddingTop: "1.5rem",

    "@media (min-width: 640px)": {
      flexDirection: "row",
    },
  },

  primaryButton: {
    flex: 1,
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    gap: "0.75rem",
    padding: "1rem 1.5rem",
    borderRadius: "0.5rem",
    fontWeight: "600",
    fontSize: "1rem",
    transition: "all 0.3s ease",
    boxShadow: "0 1px 3px 0 rgb(0 0 0 / 0.1)",
    border: "1px solid",
    cursor: "pointer",

    "&:hover:not(:disabled)": {
      boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
    },

    "&:disabled": {
      background: "#9ca3af !important",
      borderColor: "#d1d5db !important",
      color: "white !important",
      cursor: "not-allowed",
      opacity: 0.6,
    },
  },

  primaryButtonPlaying: {
    background: "#dc2626",
    color: "white",
    borderColor: "#dc2626",

    "&:hover": {
      background: "#b91c1c",
      borderColor: "#b91c1c",
    },
  },

  primaryButtonIdle: {
    background: "#3b82f6",
    color: "white",
    borderColor: "#3b82f6",

    "&:hover": {
      background: "#2563eb",
      borderColor: "#2563eb",
    },
  },

  muteButton: {
    padding: "1rem 1.5rem",
    borderRadius: "0.5rem",
    fontWeight: "600",
    fontSize: "1rem",
    transition: "all 0.3s ease",
    boxShadow: "0 1px 3px 0 rgb(0 0 0 / 0.1)",
    border: "1px solid",
    cursor: "pointer",

    "&:hover": {
      boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
    },
  },

  muteButtonMuted: {
    background: "#6b7280",
    color: "white",
    borderColor: "#6b7280",

    "&:hover": {
      background: "#4b5563",
      borderColor: "#4b5563",
    },
  },

  muteButtonUnmuted: {
    background: "#3b82f6",
    color: "white",
    borderColor: "#3b82f6",

    "&:hover": {
      background: "#2563eb",
      borderColor: "#2563eb",
    },
  },

  footer: {
    textAlign: "center",
    marginTop: "2.5rem",
  },

  footerText: {
    fontSize: "1rem",
    color: "#64748b",
    fontWeight: "500",
  },

  "@keyframes pulse": {
    "0%, 100%": {
      opacity: 1,
    },
    "50%": {
      opacity: 0.5,
    },
  },

  "@keyframes bounce": {
    "0%, 100%": {
      transform: "translateY(0)",
      animationTimingFunction: "cubic-bezier(0.8, 0, 1, 1)",
    },
    "50%": {
      transform: "translateY(-25%)",
      animationTimingFunction: "cubic-bezier(0, 0, 0.2, 1)",
    },
  },

  "@keyframes ping": {
    "75%, 100%": {
      transform: "scale(2)",
      opacity: 0,
    },
  },
});
