import { ChatIcon } from "./icons";

import useStyles from "./style";

export const ChatHeader = () => {
  const classes = useStyles();

  return (
    <header className={classes.header}>
      <span className={classes.title}>Learning-teachin-chat</span>
      <ChatIcon width={24} height={24} />
    </header>
  );
};
