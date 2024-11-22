import { useAuth } from "~/composables/useAuth";
import type { Contract } from "~/Types/Contract";

export const useContractsStore = defineStore("contract", () => {
  const api = useApi();
  const auth = useAuth();

  const contracts = ref<Contract[]>([]);

  const fetchContracts = async () => {
      const response = await auth.fetchWithToken<Contract[]>("/contracts", {
        method: "GET",
      });
      if (!response.error){
        contracts.value = response;
        return contracts.value;
      }
  };

  const fetchPDF = async (contractId: number) => {
    try {
      const response = await auth.fetchWithToken<Blob>(`/contracts/${contractId}/pdf`, {
        method: "GET",
      });
  
      if (!response.error) {
        const pdfBlobUrl = URL.createObjectURL(response);
        window.open(pdfBlobUrl, "_blank");
      }
    } catch (error) {
      console.error("Error fetching PDF:", error);
    }
  };

  const deleteContract = async (contractId: number) => {
    const response = await auth.fetchWithToken(`/contracts/${contractId}`, {
      method: "DELETE",
    });
  
    if (!response.error) {
      await fetchContracts();
      console.log(
        `Contract with ID ${contractId} deleted successfully and contracts updated`
      );
    } else {
      console.error(`Error deleting contract with ID ${contractId}:`, response.error);
      return response.error; // Või viska viga, kui vaja
    }
  };
  

  return {
    contracts,
    fetchContracts,
    fetchPDF,
    deleteContract
  };
});
