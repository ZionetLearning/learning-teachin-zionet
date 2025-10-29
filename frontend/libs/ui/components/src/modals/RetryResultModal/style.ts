import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  modalRtl: {
    "& .MuiDialog-paper": {
      direction: "rtl",
    },
  },
  modalLtr: {
    "& .MuiDialog-paper": {
      direction: "ltr",
    },
  },
  title: {
    textAlign: "center",
    paddingBottom: 8,
  },
  titleIconBox: {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 8,
  },
  titleIcon: {
    fontSize: 48,
    marginInlineEnd: 8,
  },
  titleText: {
    textAlign: "center",
  },
  content: {
    paddingTop: 8,
    paddingBottom: 16,
  },
  contentBox: {
    textAlign: "center",
  },
  actions: {
    justifyContent: "center",
    padding: 16,
    gap: 16,
  },
  button: {
    minWidth: 140,
    padding: "8px 24px",
  },
});
