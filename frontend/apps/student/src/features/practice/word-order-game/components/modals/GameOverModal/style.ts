import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  gameOverModal: {
    "& .MuiDialog-paper": {
      borderRadius: 16,
    },
  },
  gameOverModalRtl: {
    "& .MuiDialog-paper": {
      borderRadius: 16,
      direction: "rtl",
    },
  },
  gameOverModalLtr: {
    "& .MuiDialog-paper": {
      borderRadius: 16,
      direction: "ltr",
    },
  },
  gameOverTitle: {
    display: "flex",
    alignItems: "center",
    gap: 16,
  },
  gameOverContent: {
    textAlign: "center",
    paddingTop: 16,
    paddingBottom: 16,
  },
  gameOverCompletedText: {
    marginBottom: 24,
  },
  gameOverActions: {
    paddingLeft: 24,
    paddingRight: 24,
    paddingBottom: 24,
    gap: 8,
    justifyContent: "flex-start",
  },
  gameOverActionsRtl: {
    paddingLeft: 24,
    paddingRight: 24,
    paddingBottom: 24,
    gap: 8,
    direction: "rtl",
    justifyContent: "flex-start",
  },
  gameOverActionsLtr: {
    paddingLeft: 24,
    paddingRight: 24,
    paddingBottom: 24,
    gap: 8,
    direction: "ltr",
    justifyContent: "flex-start",
  },
  gameOverButtonHebrew: {
    display: "flex",
    gap: 12,
    justifyContent: "space-between",
    "& .MuiButton-startIcon": {
      marginRight: "-9.6px",
    },
  },
  gameOverButtonEnglish: {
    gap: 12,
    justifyContent: "space-between",
    "& .MuiButton-startIcon": {
      marginRight: "auto",
    },
  },
  gameOverButtonEnglishSettings: {
    paddingLeft: 12,
    paddingRight: 12,
    gap: 12,
    justifyContent: "space-between",
    "& .MuiButton-startIcon": {
      marginRight: "auto",
    },
  },
});
