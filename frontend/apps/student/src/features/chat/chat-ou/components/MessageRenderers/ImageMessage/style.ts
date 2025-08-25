import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    width: "100%",
    maxWidth: "400px",
  },

  image: {
    width: "100%",
    height: "auto",
    borderRadius: "8px",
    transition: "opacity 0.3s ease",
    cursor: "pointer",
    "&:hover": {
      opacity: "0.9",
    },
  },

  imageHidden: {
    opacity: 0,
    position: "absolute",
    visibility: "hidden",
  },

  imageVisible: {
    opacity: 1,
    position: "relative",
    visibility: "visible",
  },

  caption: {
    marginTop: "8px",
    fontSize: "13px",
    color: "#666",
    fontStyle: "italic",
    textAlign: "center",
    lineHeight: "1.4",
  },

  loadingState: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    padding: "40px 20px",
    backgroundColor: "#f8f9fa",
    borderRadius: "8px",
    border: "2px dashed #dee2e6",
  },

  loadingSpinner: {
    width: "24px",
    height: "24px",
    border: "3px solid #f3f3f3",
    borderTop: "3px solid #007bff",
    borderRadius: "50%",
    animation: "$spin 1s linear infinite",
    marginBottom: "12px",
  },

  loadingText: {
    fontSize: "14px",
    color: "#666",
  },

  errorState: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    padding: "40px 20px",
    backgroundColor: "#fff5f5",
    borderRadius: "8px",
    border: "2px dashed #fed7d7",
  },

  errorIcon: {
    fontSize: "32px",
    marginBottom: "12px",
    opacity: 0.6,
  },

  errorText: {
    fontSize: "14px",
    color: "#e53e3e",
    fontWeight: "600",
    marginBottom: "4px",
  },

  errorSubtext: {
    fontSize: "12px",
    color: "#a0a0a0",
    textAlign: "center",
  },

  "@keyframes spin": {
    "0%": { transform: "rotate(0deg)" },
    "100%": { transform: "rotate(360deg)" },
  },

  "@media (max-width: 768px)": {
    container: {
      maxWidth: "100%",
    },

    loadingState: {
      padding: "30px 15px",
    },

    errorState: {
      padding: "30px 15px",
    },

    caption: {
      fontSize: "12px",
    },
  },
});
