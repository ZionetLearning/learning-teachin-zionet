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
  useTheme,
} from "@mui/material";
import RemoveIcon from "@mui/icons-material/PersonRemove";
import { RoleChip } from "@ui-components";

type Member = {
  memberId: string;
  name: string;
  role: number; // 1 = teacher, else student
};

type Props = {
  members: Member[];
  selectedIds: Set<string>;
  onToggle: (memberId: string) => void;

  // toolbar
  onSelectAll: () => void;
  onClearAll: () => void;

  // actions
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
  const theme = useTheme();

  const selectedBg = theme.vars?.palette?.primary?.mainChannel
    ? `rgba(var(--mui-palette-primary-mainChannel)/0.08)`
    : theme.palette.action.selected;

  return (
    <Box sx={{ flex: 1, minWidth: 320 }}>
      <Typography variant="subtitle1" sx={{ mb: 1 }}>
        Current Members ({members.length})
      </Typography>

      {/* Toolbar */}
      <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
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

      {/* List */}
      <List
        dense
        sx={{
          borderRadius: 2,
          border: `${theme.palette.divider} 1px solid`,
          bgcolor:
            theme.palette.mode === "dark"
              ? "rgba(255,255,255,0.03)"
              : "background.paper",
          maxHeight: 360,
          overflow: "auto",
        }}
      >
        {members.map((m) => {
          const checked = selectedIds.has(m.memberId);
          const labelId = `member-${m.memberId}`;

          return (
            <ListItem
              key={m.memberId}
              disablePadding
              secondaryAction={
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
              }
            >
              <ListItemButton
                dense
                onClick={() => onToggle(m.memberId)}
                sx={{ pr: 8, bgcolor: checked ? selectedBg : undefined }}
              >
                <ListItemIcon sx={{ minWidth: 40 }}>
                  <Checkbox
                    edge="start"
                    checked={checked}
                    tabIndex={-1}
                    disableRipple
                    onClick={(e) => e.stopPropagation()}
                    onChange={() => onToggle(m.memberId)}
                    inputProps={{ "aria-labelledby": labelId }}
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
            </ListItem>
          );
        })}

        {members.length === 0 && (
          <Box p={2}>
            <Typography variant="body2" color="text.secondary">
              This class has no members yet.
            </Typography>
          </Box>
        )}
      </List>
    </Box>
  );
};
