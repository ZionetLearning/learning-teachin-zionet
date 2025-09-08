import { Stack, Typography } from "@mui/material";
import type { UserNotification } from "@app-providers/types";

type Props = {
  items: UserNotification[];
  emptyText?: string;
};

export const NotificationFeed = ({ items, emptyText }: Props) => (
  <Stack spacing={0.5} sx={{ mt: 1 }}>
    {items.map((n, i) => (
      <Typography key={i} variant="body2">
        {n.message} — {new Date(n.timestamp).toLocaleString()}
      </Typography>
    ))}

    {items.length === 0 && (
      <Typography variant="body2" color="text.secondary">
        {emptyText ?? "No notifications yet."}
      </Typography>
    )}
  </Stack>
);
