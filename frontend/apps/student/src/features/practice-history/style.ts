import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  headerWrapper: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    textAlign: "center",
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.10) 0%, rgba(124,77,255,0.03) 100%)",
  },
  title: {
    color: "#7c4dff",
    fontSize: 26,
    fontWeight: 700,
    letterSpacing: 0.2,
  },
  description: {
    color: "#7c4dff",
    fontSize: 16,
    opacity: 0.9,
  },
  toggleGroupWrapper: {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    gap: 12,
    marginTop: 16,
    backgroundColor: "rgba(255,255,255,0.15)", // fallback for light mode
    borderRadius: 50,
    padding: 6,
    backdropFilter: "blur(6px)",
    "@media (prefers-color-scheme: dark)": {
      backgroundColor: "rgba(0,0,0,0.4)",
    },
  },
  toggleGroup: {
    "& .MuiToggleButton-root": {
      color: "#7c4dff",
      margin: "0 6px",
      border: "none",
      textTransform: "none",
      fontWeight: 600,
      borderRadius: 999,
      transition: "all 0.25s ease",
      padding: "6px 20px",
      backgroundColor: "rgba(124,77,255,0.08)",
      "&:hover": {
        backgroundColor: "rgba(124,77,255,0.2)",
      },
      "@media (prefers-color-scheme: dark)": {
        color: "#d1b3ff",
        backgroundColor: "rgba(124,77,255,0.15)",
        "&:hover": {
          backgroundColor: "rgba(124,77,255,0.3)",
        },
      },
    },
    "& .Mui-selected": {
      color: "#fff !important",
      backgroundColor: "#7c4dff !important",
      boxShadow: "0 2px 8px rgba(124,77,255,0.4)",
      "&:hover": {
        backgroundColor: "#6f3cff !important",
        boxShadow: "0 2px 10px rgba(124,77,255,0.6)",
      },
    },
  },
});
