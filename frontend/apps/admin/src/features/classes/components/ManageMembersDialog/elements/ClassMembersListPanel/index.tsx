import {
  Box,
  Button,
  Checkbox,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import RemoveIcon from "@mui/icons-material/PersonRemove";
import { RoleChip } from "@ui-components";
import { useStyles } from "./style";

type Member = {
  memberId: string;
  name: string;
  role: number;
};

type Props = {
  members: Member[];
  selectedIds: Set<string>;
  onToggle: (memberId: string) => void;
  onSelectAll: () => void;
  onClearAll: () => void;
  onRemoveSingle: (memberId: string) => void;
  onBatchRemove: () => void;
  pendingRemove: boolean;
};

export const ClassMembersListPanel = ({
  members,
  selectedIds,
  onToggle,
  onSelectAll,
  onClearAll,
  onRemoveSingle,
  onBatchRemove,
  pendingRemove,
}: Props) => {
  const classes = useStyles();

  return (
    <Box className={classes.container}>
      <Typography variant="subtitle1" className={classes.title}>
        Current Members ({members.length})
      </Typography>

      <Stack direction="row" spacing={1} className={classes.toolbar}>
        <Button size="small" onClick={onSelectAll}>
          Select all
        </Button>
        <Button size="small" onClick={onClearAll}>
          Clear
        </Button>
        <Button
          size="small"
          variant="contained"
          color="error"
          onClick={onBatchRemove}
          disabled={pendingRemove || selectedIds.size === 0}
        >
          Remove selected ({selectedIds.size})
        </Button>
      </Stack>

      <List dense className={classes.listContainer}>
        {members.map((m) => {
          const checked = selectedIds.has(m.memberId);
          const labelId = `member-${m.memberId}`;

          return (
            <ListItem key={m.memberId} disablePadding>
              <ListItemButton
                dense
                onClick={() => onToggle(m.memberId)}
                className={
                  checked
                    ? "selected " + classes.listItemButton
                    : classes.listItemButton
                }
              >
                <ListItemIcon className={classes.listItemIcon}>
                  <Checkbox
                    edge="start"
                    checked={checked}
                    disableRipple
                    tabIndex={-1}
                    slotProps={{ input: { "aria-labelledby": labelId } }}
                    onClick={(e) => e.stopPropagation()}
                    onChange={() => onToggle(m.memberId)}
                  />
                </ListItemIcon>

                <ListItemText
                  id={labelId}
                  primary={
                    <Stack direction="row" gap={1} alignItems="center">
                      <Typography>{m.name}</Typography>
                      <RoleChip
                        role={m.role === 1 ? "teacher" : "student"}
                        data-testid="users-role-badge"
                      />
                    </Stack>
                  }
                />
              </ListItemButton>

              <Tooltip title="Remove from class">
                <span>
                  <IconButton
                    edge="end"
                    onClick={(e) => {
                      e.stopPropagation();
                      onRemoveSingle(m.memberId);
                    }}
                    disabled={pendingRemove}
                  >
                    <RemoveIcon />
                  </IconButton>
                </span>
              </Tooltip>
            </ListItem>
          );
        })}

        {members.length === 0 && (
          <Box className={classes.emptyState}>
            This class has no members yet.
          </Box>
        )}
      </List>
    </Box>
  );
};
