﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Drawing;
using System.Data;

namespace SGECA.Forms.Ventas
{
    public partial class FrmVistaNotaVenta : System.Web.UI.Page, LogManager.IObserver, LogManager.ISubject
    {
        string pagina = "FrmVistaComprobante";
        int paginaActual = 1;
        int totalPaginas;
        DAL.ItemOrden[] orden;
        DAL.ItemFiltro[] itemFiltro;
        bool busquedaAnd;
        IList datosObtenidos;
        double registroInicio, registroFin, cantidadRegistros;
        LogManager.Mensaje UltimoMensaje { get; set; }

        public void Page_Load(object sender, EventArgs e)
        {
            DAL.ComprobanteEncabezado enc = new DAL.ComprobanteEncabezado();

            this.Page.Master.EnableViewState = true;

            if (!IsPostBack)
                cargarDatosFiltro();


            recuperaVariables();

            BusquedaConFiltro.LabelDescripcion = "Buscar por nùmero de Comprobante:";
            BusquedaConFiltro.campoDescripcion = "cen_numerocompleto";
            BusquedaConFiltro.Error -= BusquedaConFiltro_Error;
            BusquedaConFiltro.Filtrar -= BusquedaConFiltro_Filtrar;
            BusquedaConFiltro.Error += BusquedaConFiltro_Error;
            BusquedaConFiltro.Filtrar += BusquedaConFiltro_Filtrar;

            if (Session[pagina + "datosObtenidos"] != null)
                cargarGrilla();


            //if (!Page.IsPostBack)
            //{
            //grdComprobantes.DataSource = enc.obtener();
            //    grdComprobantes.DataKeyNames = new string[] { "id" };
            //    grdComprobantes.DataBind();
            //}

        }

        #region Observer Pattern
        private List<object> Observers = new List<object>();

        /// <summary>
        /// Método encargado de recibir notificaciones del subscriptor donde  ha sucedido un evento que 
        /// requiere su atención.
        /// </summary>
        public void UpdateState(LogManager.IMensaje mensaje)
        {
            Notify(mensaje);
        }

        /// <summary>
        /// Método encargado de notificar al subscriptor que ha sucedido un evento que 
        /// requiere su atención.
        /// </summary>
        public void Notify(LogManager.IMensaje mensaje)
        {
            // Recorremos cada uno de los observadores para notificarles el evento.
            foreach (LogManager.IObserver observer in this.Observers)
            {
                // Indicamos a cada uno de los subscriptores la actualización del 
                // estado (evento) producido.
                observer.UpdateState(mensaje);
            }
        } // Notify

        /// <summary>
        /// Método encargado de agregar un observador para que el subscriptor le 
        /// pueda notificar al subscriptor el evento.
        /// </summary>
        /// <param name="observer">Interfaz IObserver que indica el observador.</param>
        public void Subscribe(LogManager.IObserver observer)
        {
            if (!this.Observers.Contains(observer))
                // Agregamos el subscriptor a la lista de subscriptores del publicador.
                this.Observers.Add(observer);
        } // Subscribe


        /// <summary>
        /// Método encargado de eliminar un observador para que el subscriptor no le 
        /// notifique ningún evento más al que era su subscriptor.
        /// </summary>
        /// <param name="observer">Interfaz IObserver que indica el observador.</param>
        public void Unsubscribe(LogManager.IObserver observer)
        {
            // Eliminamos el subscriptor de la lista de subscriptores del publicador.
            this.Observers.Remove(observer);
        } // Unsubscribe

        #endregion


        protected void grdComprobantes_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = 0;
            int.TryParse(grdComprobantes.SelectedDataKey.Value.ToString(), out id);
            if (id != 0)
            {
                DAL.ComprobanteEncabezado cmp = new DAL.ComprobanteEncabezado();
                cmp.obtener(id);

                Session["ComprobanteEncabezado"] = cmp;

                Response.Redirect("FrmVentas.aspx?action=view");
                //imprimirComprobante(id);
            }



        }

        public void imprimirComprobante(int id)
        {
            //DAL.ComprobanteEncabezado imp = new DAL.ComprobanteEncabezado();
            //imp.imprimir(id);

            //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('El documento fue puesto en cola de impresión...');", true);
        }


        private void recuperaVariables()
        {

            if (Session[pagina + "paginaActual"] != null)
                paginaActual = Convert.ToInt32(Session[pagina + "paginaActual"]);
            if (Session[pagina + "totalPaginas"] != null)
                totalPaginas = Convert.ToInt32(Session[pagina + "totalPaginas"]);
            if (Session[pagina + "orden"] != null)
                orden = (DAL.ItemOrden[])Session[pagina + "orden"];
            if (Session[pagina + "itemFiltro"] != null)
                itemFiltro = (DAL.ItemFiltro[])Session[pagina + "itemFiltro"];
            if (Session[pagina + "busquedaAnd"] != null)
                busquedaAnd = (bool)Session[pagina + "busquedaAnd"];
            if (Session[pagina + "datosObtenidos"] != null)
                datosObtenidos = (IList)Session[pagina + "datosObtenidos"];
            if (Session[pagina + "registroInicio"] != null)
                registroInicio = (double)Session[pagina + "registroInicio"];
            if (Session[pagina + "registroFin"] != null)
                registroFin = (double)Session[pagina + "registroFin"];
            if (Session[pagina + "cantidadRegistros"] != null)
                cantidadRegistros = (double)Session[pagina + "cantidadRegistros"];

        }

        public void cargarDatosFiltro()
        {
            List<DAL.ItemBusqueda> items = new List<DAL.ItemBusqueda>();
            DAL.ItemBusqueda li1 = new DAL.ItemBusqueda("Cliente", "cli_RazonSocial",
                                                                    DAL.ItemBusqueda.TipoCampo._string);
            DAL.ItemBusqueda li2 = new DAL.ItemBusqueda("Fecha", "cen_fecha",
                                                                    DAL.ItemBusqueda.TipoCampo._string);
            DAL.ItemBusqueda li3 = new DAL.ItemBusqueda("CAE Nº", "cen_cae",
                                                                    DAL.ItemBusqueda.TipoCampo._string);


            items.Add(li1);
            items.Add(li2);
            items.Add(li3);

            BusquedaConFiltro.campos = items;
            BusquedaConFiltro.campoActual = "cen_numerocompleto";
            BusquedaConFiltro.tipoFiltroActual = DAL.TipoFiltro.Like;




        }

        void BusquedaConFiltro_Filtrar(DAL.ItemFiltro[] itemFiltro, bool busquedaAnd)
        {
            Session[pagina + "paginaActual"] = 1;

            Session[pagina + "itemFiltro"] = itemFiltro;
            Session[pagina + "busquedaAnd"] = busquedaAnd;

            DAL.ItemOrden[] orden = new DAL.ItemOrden[1];
            orden[0] = new DAL.ItemOrden();
            orden[0].Campo = "cen_fecha";
            orden[0].TipoOrden = DAL.TipoOrden.Ascendente;
            Session[pagina + "orden"] = orden;

            obtenerDatosFiltrados(false);
        }


        private void obtenerDatosFiltrados(bool todos)
        {
            paginaActual = (int)Session[pagina + "paginaActual"];
            int tamañoPagina = pagPaginador.obtenerRegistrosMostrar();

            registroInicio = ((paginaActual - 1) * tamañoPagina) + 1;

            if (todos)
                registroFin = -1;
            else
                registroFin = tamañoPagina * paginaActual;

            DAL.IVistaComprobantes VistaComprobantes = new DAL.VistaComprobantes();

            VistaComprobantes.Subscribe(this);

            datosObtenidos = VistaComprobantes.obtenerFiltrado((DAL.ItemFiltro[])Session[pagina + "itemFiltro"],
                                                  (DAL.ItemOrden[])Session[pagina + "orden"],
                                                  (bool)Session[pagina + "busquedaAnd"],
                                                  registroInicio,
                                                  registroFin,
                                                  out  cantidadRegistros);
            if (VistaComprobantes.UltimoMensaje != null)
            {
                UltimoMensaje = VistaComprobantes.UltimoMensaje;
                Notify(UltimoMensaje);
                return;
            }

            ArrayList lista = new ArrayList();
            foreach (DAL.IVistaComprobantes item in datosObtenidos)
            {
                var itemLista = new
                {
                    ID = item.Id,
                    NumeroCompleto = item.NumeroCompleto,
                    RazonSocial = item.RazonSocial,
                    IVA = item.IVA,
                    Neto = item.Neto,
                    Total = item.Total,
                    Fecha = item.Fecha
                };
                lista.Add(itemLista);
            }




            Session[pagina + "datosObtenidos"] = datosObtenidos;
            Session[pagina + "totalPaginas"] = totalPaginas;
            Session[pagina + "totalPaginas"] = totalPaginas;
            cargarGrilla();

            calcularTotalPaginas(tamañoPagina);

            //verificarPosibilidadPaginacion();

            pagPaginador.setPaginaActual(paginaActual);

            //if (datosObtenidos.Count > 0)
            //    interfaz.habilitaExportacion();
            //else
            //    interfaz.dehabilitaExportacion();
        }

        private void cargarGrilla()
        {
            grdComprobantes.DataSource = Session[pagina + "datosObtenidos"];
            grdComprobantes.DataBind();

        }

        private void calcularTotalPaginas(int tamañoPagina)
        {
            totalPaginas = (int)Math.Ceiling(int.Parse(cantidadRegistros.ToString()) / (double)tamañoPagina);
            Session[pagina + "totalPaginas"] = totalPaginas;

            pagPaginador.setCantidadRegistros(cantidadRegistros);
            pagPaginador.setTotalPaginas(totalPaginas);
        }


        void BusquedaConFiltro_Error(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        private void cargaGrilla(object origenDatos)
        {
            grdComprobantes.DataSource = origenDatos;
            grdComprobantes.DataKeyNames = new string[] { "Id" };
            grdComprobantes.DataBind();
        }

        protected void grdGrilla_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (grdComprobantes.SelectedValue != null)
            {
                DAL.VistaComprobantes p = new DAL.VistaComprobantes();
                p.obtener((int)grdComprobantes.SelectedValue);
                TextBox t = new TextBox();
                t.Text = p.NumeroCompleto;
                this.Form.Controls.Add(t);
            }
        }

        protected void grdGrilla_SelectedIndexChanging(object sender, GridViewSelectEventArgs e)
        {
            grdComprobantes.SelectedIndex = e.NewSelectedIndex;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Session["VistaComprobante"] = null;
            cargaGrilla(DAL.VistaComprobantes.obtener());
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            DAL.VistaComprobantes p = new DAL.VistaComprobantes();
            //p.NumeroCompleto = DateTime.Now.ToString();
            List<DAL.VistaComprobantes> misVistaComprobante = new List<DAL.VistaComprobantes>();

            if (Session["VistaComprobante"] != null && Session["VistaComprobante"] is List<DAL.VistaComprobantes>)
            {
                misVistaComprobante = (List<DAL.VistaComprobantes>)Session["VistaComprobante"];
            }

            misVistaComprobante.Add(p);

            Session["VistaComprobante"] = misVistaComprobante;

            cargaGrilla(misVistaComprobante);

        }

        protected void grdGrilla_PageIndexChanged(object sender, EventArgs e)
        {


            cargaGrilla(Session["VistaComprobante"]);
        }

        protected void grdGrilla_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdComprobantes.PageIndex = e.NewPageIndex;
        }

        protected void pagPaginador_Anterior()
        {
            Session[pagina + "paginaActual"] = ((int)Session[pagina + "paginaActual"]) - 1;
            obtenerDatosFiltrados(false);

            cargarGrilla();
        }

        protected void pagPaginador_Fin()
        {

            Session[pagina + "paginaActual"] = Session[pagina + "totalPaginas"];
            obtenerDatosFiltrados(false);

            cargarGrilla();
        }

        protected void pagPaginador_Inicio()
        {
            Session[pagina + "paginaActual"] = 1;
            obtenerDatosFiltrados(false);

            cargarGrilla();
        }

        protected void pagPaginador_Proxima()
        {

            Session[pagina + "paginaActual"] = ((int)Session[pagina + "paginaActual"]) + 1;
            obtenerDatosFiltrados(false);

            cargarGrilla();
        }

        protected void pagPaginador_PaginaSeleccionada(int paginaActual)
        {

            Session[pagina + "paginaActual"] = paginaActual;
            obtenerDatosFiltrados(false);

            cargarGrilla();
        }

        protected void grdComprobantes_Sorted(object sender, EventArgs e)
        {

        }

        protected void grdComprobantes_Sorting(object sender, GridViewSortEventArgs e)
        {
            Session[pagina + "paginaActual"] = 1;

            Session[pagina + "itemFiltro"] = itemFiltro;
            Session[pagina + "busquedaAnd"] = busquedaAnd;

            if (this.orden != null)
            {
                if (this.orden[0].Campo.ToUpper() == e.SortExpression.ToUpper())
                    if (this.orden[0].TipoOrden == DAL.TipoOrden.Ascendente)
                        this.orden[0].TipoOrden = DAL.TipoOrden.Descendente;
                    else
                        this.orden[0].TipoOrden = DAL.TipoOrden.Ascendente;
                else
                {
                    orden[0].Campo = e.SortExpression;
                    orden[0].TipoOrden = DAL.TipoOrden.Ascendente;
                }
            }
            else
            {

                this.orden = new DAL.ItemOrden[1];
                orden[0] = new DAL.ItemOrden();
                orden[0].Campo = e.SortExpression;
                orden[0].TipoOrden = DAL.TipoOrden.Ascendente;
            }


            Session[pagina + "orden"] = orden;

            obtenerDatosFiltrados(false);
        }



    }
}