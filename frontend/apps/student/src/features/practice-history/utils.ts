export const getStatusColor = (status: string) => {
  switch (status) {
    case "Success":
      return "success";
    case "Failure":
      return "error";
    case "Pending":
      return "warning";
    default:
      return "default";
  }
};

export const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString("he-IL", {
    day: "2-digit",
    month: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
};