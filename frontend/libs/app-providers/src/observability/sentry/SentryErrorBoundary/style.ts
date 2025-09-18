import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  // Main container with full-screen gradient
  container: {
    minHeight: "100vh",
    background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    overflow: "hidden",
  },

  paper: {
    padding: 64,
    marginBottom: 24,
    textAlign: "center",
    borderRadius: 24,
    backgroundColor: "rgba(255, 255, 255, 0.1)",
    backdropFilter: "blur(10px)",
    border: "1px solid rgba(255, 255, 255, 0.2)",
    color: "white",
    position: "relative",
  },

  // Content wrapper
  contentBox: {
    position: "relative",
    zIndex: 1,
  },

  // Error icon container
  iconContainer: {
    marginBottom: 24,
    display: "flex",
    justifyContent: "center",
  },

  // Error icon
  errorIcon: {
    fontSize: 80,
    color: "#FFD700",
    filter: "drop-shadow(0 4px 8px rgba(0,0,0,0.3))",
  },

  // Main title
  title: {
    fontWeight: "bold",
    marginBottom: 32,
    textShadow: "0 2px 4px rgba(0,0,0,0.3)",
  },

  // Subtitle message
  message: {
    marginBottom: 40,
    opacity: 0.9,
    maxWidth: 600,
    margin: "0 auto 40px auto",
    lineHeight: 1.6,
  },

  // Button stack container
  buttonStack: {
    marginBottom: 24,
    paddingTop: 8,
  },

  // Base button styles
  baseButton: {
    color: "white",
    fontWeight: "bold",
    minWidth: 180,
    paddingLeft: 32,
    paddingRight: 32,
    paddingTop: 12,
    paddingBottom: 12,
    transition: "all 0.3s ease",
  },

  // Outlined button (Go Back)
  outlinedButton: {
    borderColor: "rgba(255, 255, 255, 0.5)",
    "&:hover": {
      borderColor: "white",
      backgroundColor: "rgba(255, 255, 255, 0.1)",
      transform: "translateY(-2px)",
    },
  },

  // Contained button (Try Again)
  containedButton: {
    backgroundColor: "rgba(255, 255, 255, 0.2)",
    backdropFilter: "blur(10px)",
    border: "1px solid rgba(255, 255, 255, 0.3)",
    "&:hover": {
      backgroundColor: "rgba(255, 255, 255, 0.3)",
      transform: "translateY(-2px)",
      boxShadow: "0 8px 25px rgba(0,0,0,0.3)",
    },
  },

  // Floating animation element
  floatingElement: {
    position: "absolute",
    top: -50,
    right: -50,
    width: 100,
    height: 100,
    borderRadius: "50%",
    background: "rgba(255, 255, 255, 0.1)",
    animation: "$float 6s ease-in-out infinite",
  },

  // Animation keyframes
  "@keyframes float": {
    "0%, 100%": { transform: "translateY(0px)" },
    "50%": { transform: "translateY(-20px)" },
  },
});
