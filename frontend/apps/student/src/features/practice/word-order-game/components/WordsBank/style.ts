import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  bankWrapper: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    gap: 8,
    width: "100%",
  },
  bankHeader: {
    fontSize: 20,
    fontWeight: 600,
    color: "#6f42c1",
    marginBottom: 4,
    textShadow: "0 1px 2px rgba(124,77,255,0.08)",
  },
  wordsBank: ({ isEmpty }: { isEmpty: boolean }) => ({
    display: "flex",
    flexWrap: "wrap",
    justifyContent: "center",
    alignItems: "center",
    gap: 8,
    background: isEmpty
      ? "transparent"
      : "linear-gradient(180deg, #fafbfc 0%, #f5f7fa 100%)",
    border: isEmpty ? "none" : "1px solid rgba(180, 200, 210, 0.4)",
    borderRadius: 16,
    padding: "12px 16px",
    minHeight: 64,
    boxShadow: isEmpty ? "none" : "inset 0 1px 2px rgba(0,0,0,0.04)",
  }),
  bankWord: {
    padding: "8px 16px",
    borderRadius: 16,
    border: "1px solid rgba(180, 200, 210, 0.6)",
    background: "linear-gradient(145deg, #ffffff, #edf3f5)",
    color: "#1f3a46",
    fontSize: 18,
    fontWeight: 500,
    boxShadow: "0 2px 5px rgba(0, 0, 0, 0.06)",
    cursor: "pointer",
    transition: "all 0.2s ease",
    "&:hover": {
      background: "linear-gradient(145deg, #eaf3f6, #dce9ec)",
      transform: "translateY(-2px)",
      boxShadow: "0 4px 8px rgba(0, 0, 0, 0.12)",
    },
    "&:active": {
      transform: "scale(0.97)",
      boxShadow: "0 2px 4px rgba(0, 0, 0, 0.15)",
    },
  },
  bankHint: {
    marginTop: 4,
    fontSize: 14,
    color: "rgba(60, 60, 80, 0.7)",
    fontStyle: "italic",
  },
  errorText: {
    color: "red",
    fontWeight: 500,
  },
});
