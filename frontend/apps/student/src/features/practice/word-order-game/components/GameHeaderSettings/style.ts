import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  gameHeader: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 16,
    paddingHorizontal: 8,
    gap: 16,
  },
  gameHeaderInfo: {
    flexDirection: "column",
  },
  settingsButtonEnglish: {
    minWidth: 100,
    whiteSpace: "nowrap",
  },
  settingsButtonHebrew: {
    minWidth: 100,
    whiteSpace: "nowrap",
    gap: 8,
    "& .MuiButton-startIcon": {
      marginRight: "-15px",
    },
  },
});
